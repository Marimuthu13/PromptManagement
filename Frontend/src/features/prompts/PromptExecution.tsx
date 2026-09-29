import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { promptApi } from '../../services/api/promptApi';
import type { PromptDto, PromptExecutionDto } from '../../services/types/prompt';
import './PromptExecution.css';

const PromptExecution: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const [prompt, setPrompt] = useState<PromptDto | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');

  // Form states
  const [providerName, setProviderName] = useState('Gemini');
  const [modelName, setModelName] = useState('gemini-3.6-flash');
  const [variables, setVariables] = useState<Record<string, string>>({});

  // Execution states
  const [isExecuting, setIsExecuting] = useState(false);
  const [executionResult, setExecutionResult] = useState<PromptExecutionDto | null>(null);

  // Autofill states
  const [isGenerating, setIsGenerating] = useState(false);

  // Copy states
  const [copied, setCopied] = useState(false);

  useEffect(() => {
    const fetchPrompt = async () => {
      if (!id) return;
      setIsLoading(true);
      setError('');
      try {
        const data = await promptApi.getPromptById(id);
        setPrompt(data);

        // Initialize variables
        const initialVars: Record<string, string> = {};
        data.variables.forEach(v => {
          initialVars[v.name] = '';
        });
        setVariables(initialVars);
      } catch (err) {
        setError('Failed to load prompt for execution.');
      } finally {
        setIsLoading(false);
      }
    };
    fetchPrompt();
  }, [id]);

  const handleVariableChange = (name: string, value: string) => {
    setVariables(prev => ({ ...prev, [name]: value }));
  };

  const handleExecute = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!id) return;

    setIsExecuting(true);
    setExecutionResult(null);
    setError('');

    try {
      const result = await promptApi.executePrompt(id, {
        promptId: id,
        providerName,
        modelName,
        variables
      });
      setExecutionResult(result);
    } catch (err: any) {
      setError(err.response?.data?.title || 'Execution failed.');
    } finally {
      setIsExecuting(false);
    }
  };

  const handleAutofill = async () => {
    if (!prompt || prompt.variables.length === 0) return;

    setIsGenerating(true);
    setError('');

    try {
      const variableNames = prompt.variables.map(v => v.name);
      const generatedValues = await promptApi.generateVariables(prompt.content, variableNames);

      setVariables(prev => ({
        ...prev,
        ...generatedValues
      }));
    } catch (err: any) {
      setError(err.response?.data?.title || 'Failed to generate variable values.');
    } finally {
      setIsGenerating(false);
    }
  };

  const handleCopyResult = async () => {
    if (!executionResult?.resultContent) return;
    try {
      await navigator.clipboard.writeText(executionResult.resultContent);
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    } catch (err) {
      console.error('Failed to copy text: ', err);
    }
  };

  if (isLoading) return <div className="execution-loading">Loading prompt...</div>;
  if (!prompt) return <div className="execution-error">{error || 'Prompt not found.'}</div>;

  return (
    <div className="prompt-execution-container">
      <div className="execution-header">
        <h2>Execute: {prompt.title}</h2>
        <button type="button" className="back-btn" onClick={() => navigate('/')}>
          Back to Library
        </button>
      </div>

      {error && <div className="execution-error">{error}</div>}

      <div className="execution-layout">
        <div className="execution-config">
          <form onSubmit={handleExecute} className="execution-form">
            <div className="form-group">
              <label htmlFor="provider">AI Provider</label>
              <select
                id="provider"
                value={providerName}
                onChange={e => setProviderName(e.target.value)}
              >
                <option value="Gemini">Gemini</option>
                <option value="OpenAI">OpenAI</option>
                {/* Future providers can be added here */}
              </select>
            </div>

            <div className="form-group">
              <label htmlFor="model">Model Name</label>
              <input
                type="text"
                id="model"
                value={modelName}
                onChange={e => setModelName(e.target.value)}
              />
            </div>

            {prompt.variables.length > 0 && (
              <div className="variables-section">
                <div className="variables-header-row">
                  <h3>Prompt Variables</h3>
                  <button
                    type="button"
                    className="autofill-btn"
                    onClick={handleAutofill}
                    disabled={isGenerating}
                  >
                    {isGenerating ? 'Generating...' : '✨ Autofill Sample Values'}
                  </button>
                </div>
                {prompt.variables.map(v => (
                  <div className="form-group" key={v.id}>
                    <label htmlFor={`var-${v.name}`}>
                      {v.name} {v.isRequired && <span className="required">*</span>}
                    </label>
                    <textarea
                      id={`var-${v.name}`}
                      className={isGenerating ? 'skeleton-pulse' : ''}
                      value={variables[v.name] || ''}
                      onChange={e => handleVariableChange(v.name, e.target.value)}
                      required={v.isRequired}
                      rows={3}
                      disabled={isGenerating}
                    />
                  </div>
                ))}
              </div>
            )}

            <button type="submit" className="execute-submit-btn" disabled={isExecuting}>
              {isExecuting ? 'Executing...' : 'Execute Prompt'}
            </button>
          </form>
        </div>

        <div className="execution-result-panel">
          <h3>Execution Result</h3>
          {isExecuting ? (
            <div className="result-loading">Waiting for AI response...</div>
          ) : executionResult ? (
            <div className={`result-card ${executionResult.isSuccessful ? 'success' : 'error'}`}>
              {executionResult.isFallback && (
                <div className="fallback-banner">
                  ⚠️ AI provider unreachable. Result generated using local fallback mode.
                </div>
              )}
              <div className="result-header-row">
                <div className="result-metadata">
                  <span className="metadata-badge">{executionResult.model}</span>
                  <span className="metadata-badge">{executionResult.tokensUsed} tokens</span>
                  <span className="metadata-badge">{executionResult.durationMs} ms</span>
                </div>
                {executionResult.isSuccessful && (
                  <button
                    type="button"
                    className={`copy-result-btn ${copied ? 'copied' : ''}`}
                    onClick={handleCopyResult}
                  >
                    {copied ? '✓ Copied!' : '📋 Copy'}
                  </button>
                )}
              </div>
              <div className="result-content">
                {executionResult.isSuccessful ? (
                  <pre>{executionResult.resultContent}</pre>
                ) : (
                  <div className="error-message">
                    <strong>Error:</strong> {executionResult.errorMessage}
                  </div>
                )}
              </div>
            </div>
          ) : (
            <div className="result-placeholder">
              Configure parameters and click "Execute Prompt" to see the result here.
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default PromptExecution;
