import React, { useEffect, useState } from 'react';
import { useLocation, useNavigate, useParams } from 'react-router-dom';
import { promptApi } from '../../services/api/promptApi';
import { categoryApi } from '../../services/api/categoryApi';
import type { CategoryDto } from '../../services/types/category';
import type { PromptVersionDto, PromptAnalysisDto } from '../../services/types/prompt';
import './PromptEditor.css';

const PromptEditor: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const location = useLocation();
  const isEditMode = !!id;
  const isTemplate = new URLSearchParams(location.search).get('isTemplate') === 'true';

  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [content, setContent] = useState('');
  const [categoryId, setCategoryId] = useState('');

  const [categories, setCategories] = useState<CategoryDto[]>([]);
  const [versions, setVersions] = useState<PromptVersionDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState('');
  
  // Prompt Doctor State
  const [analysis, setAnalysis] = useState<PromptAnalysisDto | null>(null);
  const [isAnalyzing, setIsAnalyzing] = useState(false);

  useEffect(() => {
    const initForm = async () => {
      setIsLoading(true);
      setError('');
      try {
        const categoriesData = await categoryApi.getCategories();
        setCategories(categoriesData);
        if (categoriesData.length > 0 && !isEditMode) {
          setCategoryId(categoriesData[0].id);
        }

        if (isEditMode && id) {
          const [promptData, versionsData] = await Promise.all([
            promptApi.getPromptById(id),
            promptApi.getPromptVersions(id)
          ]);
          setTitle(promptData.title);
          setDescription(promptData.description);
          setContent(promptData.content);
          setCategoryId(promptData.categoryId);
          setVersions(versionsData);
        }
      } catch (err) {
        setError('Failed to initialize editor');
      } finally {
        setIsLoading(false);
      }
    };
    initForm();
  }, [id, isEditMode]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSubmitting(true);
    setError('');

    try {
      if (isEditMode && id) {
        await promptApi.updatePrompt(id, { id, title, description, content, categoryId });
      } else {
        await promptApi.createPrompt({ title, description, content, categoryId, isTemplate });
      }
      navigate(isTemplate ? '/templates' : '/');
    } catch (err: any) {
      setError(err.response?.data?.title || 'Failed to save prompt');
      setIsSubmitting(false);
    }
  };

  const handleDelete = async () => {
    if (!id || !window.confirm('Are you sure you want to delete this prompt?')) return;

    setIsSubmitting(true);
    try {
      await promptApi.deletePrompt(id);
      navigate('/');
    } catch (err: any) {
      setError(err.response?.data?.title || 'Failed to delete prompt');
      setIsSubmitting(false);
    }
  };

  const handleRestore = async (versionId: string) => {
    if (!id) return;
    if (!window.confirm('Are you sure you want to restore this version? Unsaved changes will be lost.')) return;

    setIsSubmitting(true);
    setError('');
    try {
      const restoredPrompt = await promptApi.restorePromptVersion(id, versionId);
      setContent(restoredPrompt.content);
      // Refresh versions to show the new one
      const versionsData = await promptApi.getPromptVersions(id);
      setVersions(versionsData);
      alert('Version restored successfully!');
    } catch (err: any) {
      setError(err.response?.data?.title || 'Failed to restore version');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleAnalyze = async () => {
    if (!content.trim()) {
      setError('Please enter some prompt content to analyze.');
      return;
    }

    setIsAnalyzing(true);
    setError('');
    try {
      const result = await promptApi.optimizePrompt(content);
      setAnalysis(result);
    } catch (err: any) {
      setError(err.response?.data?.title || 'Failed to analyze prompt');
    } finally {
      setIsAnalyzing(false);
    }
  };

  const handleApplyAnalysis = () => {
    if (analysis) {
      setContent(analysis.optimizedPrompt);
      setAnalysis(null);
    }
  };

  if (isLoading) {
    return <div className="editor-loading">Loading editor...</div>;
  }

  return (
    <div className="prompt-editor-container">
      <div className="editor-header">
        <h2>{isEditMode ? 'Edit Prompt' : (isTemplate ? 'Create New Template' : 'Create New Prompt')}</h2>
        <div className="editor-actions">
          {isEditMode && (
            <button type="button" className="delete-btn" onClick={handleDelete} disabled={isSubmitting}>
              Delete
            </button>
          )}
          <button type="button" className="cancel-btn" onClick={() => navigate(isTemplate ? '/templates' : '/')} disabled={isSubmitting}>
            Cancel
          </button>
        </div>
      </div>

      {error && <div className="editor-error">{error}</div>}

      <div className="editor-layout">
        <form onSubmit={handleSubmit} className="editor-form">
          <div className="form-group">
            <label htmlFor="title">Title</label>
            <input
              id="title"
              type="text"
              value={title}
              onChange={e => setTitle(e.target.value)}
              required
              placeholder="e.g., Explain concept clearly"
            />
          </div>

          <div className="form-group">
            <label htmlFor="category">Category</label>
            <select
              id="category"
              value={categoryId}
              onChange={e => setCategoryId(e.target.value)}
              required
            >
              <option value="" disabled>Select a category</option>
              {categories.map(c => (
                <option key={c.id} value={c.id}>{c.name}</option>
              ))}
            </select>
          </div>

          <div className="form-group">
            <label htmlFor="description">Description</label>
            <input
              id="description"
              type="text"
              value={description}
              onChange={e => setDescription(e.target.value)}
              placeholder="Briefly describe what this prompt does"
            />
          </div>

          <div className="form-group flex-grow">
            <div className="content-header-row">
              <label htmlFor="content">Prompt Content</label>
              <button 
                type="button" 
                className="analyze-trigger-btn"
                onClick={handleAnalyze}
                disabled={isAnalyzing || !content.trim()}
              >
                {isAnalyzing ? 'Analyzing...' : '🩺 Analyze & Optimize (Prompt Doctor)'}
              </button>
            </div>
            <textarea
              id="content"
              value={content}
              onChange={e => setContent(e.target.value)}
              required
              placeholder="Enter your prompt text here... use {{Variable}} for placeholders."
              rows={15}
            />
            {analysis && (
              <div className="doctor-report-panel">
                <div className="doctor-report-header">
                  <h3>Doctor's Report</h3>
                  <span className={`grade-badge grade-${analysis.grade.toLowerCase()}`}>
                    Grade: {analysis.grade} ({analysis.score}/100)
                  </span>
                </div>
                
                <div className="doctor-report-section">
                  {analysis.isFallback && (
                    <div className="fallback-banner">
                      ⚠️ AI provider unreachable. Result generated using local fallback mode.
                    </div>
                  )}
                  <h4>Suggestions</h4>
                  <ul>
                    {analysis.suggestions.map((sug, i) => (
                      <li key={i}>{sug}</li>
                    ))}
                  </ul>
                </div>
                
                <div className="doctor-report-section">
                  <h4>Optimized Prompt</h4>
                  <div className="optimized-prompt-text">{analysis.optimizedPrompt}</div>
                </div>
                
                <button 
                  type="button" 
                  className="apply-optimized-btn"
                  onClick={handleApplyAnalysis}
                >
                  Apply Optimized Version
                </button>
              </div>
            )}
          </div>

          <button type="submit" className="save-btn" disabled={isSubmitting}>
            {isSubmitting ? 'Saving...' : (isEditMode ? 'Save Changes' : 'Create Prompt')}
          </button>
        </form>

        {isEditMode && (
          <aside className="editor-sidebar">
            <h3>Version History</h3>
            <div className="version-list">
              {versions.length === 0 ? (
                <p className="no-versions">No version history available.</p>
              ) : (
                versions.map(version => (
                  <div className="version-item" key={version.id}>
                    <div className="version-info">
                      <span className="version-number">v{version.versionNumber}</span>
                      <span className="version-date">{new Date(version.createdAtUtc).toLocaleString()}</span>
                    </div>
                    <button
                      type="button"
                      className="restore-btn"
                      onClick={() => handleRestore(version.id)}
                      disabled={isSubmitting}
                    >
                      Restore
                    </button>
                  </div>
                ))
              )}
            </div>
          </aside>
        )}
      </div>
    </div>
  );
};

export default PromptEditor;
