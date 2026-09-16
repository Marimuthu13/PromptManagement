import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { templateApi } from '../../services/api/templateApi';
import type { TemplateDto } from '../../services/types/template';
import type { PagedResult } from '../../services/types/common';
import './Templates.css';

const Templates: React.FC = () => {
  const navigate = useNavigate();
  const [templatesPage, setTemplatesPage] = useState<PagedResult<TemplateDto> | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');
  
  const [searchInput, setSearchInput] = useState('');
  const [page, setPage] = useState(1);

  const fetchTemplates = async (pageNum: number, search?: string) => {
    setIsLoading(true);
    setError('');
    try {
      const data = await templateApi.getTemplates({
        page: pageNum,
        pageSize: 12,
        searchText: search
      });
      setTemplatesPage(data);
    } catch (err: any) {
      setError(err.response?.data?.title || 'Failed to load templates.');
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchTemplates(page, searchInput);
  }, [page]); // Re-fetch on page change

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    setPage(1);
    fetchTemplates(1, searchInput);
  };

  const handleDuplicate = async (id: string) => {
    try {
      const newPromptId = await templateApi.duplicateTemplate(id);
      navigate(`/prompts/${newPromptId}/edit`);
    } catch (err: any) {
      alert(err.response?.data?.title || 'Failed to duplicate template');
    }
  };

  return (
    <div className="templates-container">
      <div className="templates-header">
        <h2>Curated Templates</h2>
        <form className="search-bar" onSubmit={handleSearch}>
          <input 
            type="text" 
            placeholder="Search templates..." 
            value={searchInput}
            onChange={(e) => setSearchInput(e.target.value)}
          />
          <button type="submit">Search</button>
        </form>
      </div>

      {error && <div className="templates-error">{error}</div>}

      {isLoading ? (
        <div className="templates-loading">Loading templates...</div>
      ) : templatesPage && templatesPage.items.length > 0 ? (
        <>
          <div className="templates-grid">
            {templatesPage.items.map(template => (
              <div className="template-card" key={template.id}>
                <div className="template-card-header">
                  <h3>{template.title}</h3>
                  <span className="category-badge">{template.categoryName}</span>
                </div>
                <p className="template-description">{template.description}</p>
                <div className="template-actions">
                  <button onClick={() => navigate(`/prompts/${template.id}/execute`)} className="execute-btn" style={{ marginRight: '10px' }}>Execute</button>
                  <button onClick={() => handleDuplicate(template.id)} className="duplicate-btn">Clone to Library</button>
                </div>
              </div>
            ))}
          </div>

          {templatesPage.totalCount > 0 && (
            <div className="pagination">
              <button 
                disabled={page === 1}
                onClick={() => setPage(p => p - 1)}
              >
                Previous
              </button>
              <span>Page {page}</span>
              <button 
                disabled={page * 12 >= templatesPage.totalCount}
                onClick={() => setPage(p => p + 1)}
              >
                Next
              </button>
            </div>
          )}
        </>
      ) : (
        <div className="templates-empty">
          <svg className="empty-icon" fill="none" stroke="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 7v10a2 2 0 002 2h14a2 2 0 002-2V9a2 2 0 00-2-2h-6l-2-2H5a2 2 0 00-2 2z" />
          </svg>
          <h3>No templates found</h3>
          <p>Try adjusting your search or create a new template to get started.</p>
          <button className="create-template-btn" onClick={() => navigate('/prompts/new?isTemplate=true')}>Create Template</button>
        </div>
      )}
    </div>
  );
};

export default Templates;
