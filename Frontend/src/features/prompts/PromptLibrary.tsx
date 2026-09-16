import React, { useEffect, useState, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import type { PromptDto } from '../../services/types/prompt';
import type { PagedResult } from '../../services/types/common';
import { promptApi } from '../../services/api/promptApi';
import CategorySidebar from '../categories/CategorySidebar';
import PromptCard from './PromptCard';
import './PromptLibrary.css';

const PromptLibrary: React.FC = () => {
  const navigate = useNavigate();
  const [promptsPage, setPromptsPage] = useState<PagedResult<PromptDto> | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');

  // Filters
  const [categoryId, setCategoryId] = useState<string | undefined>(undefined);
  const [searchText, setSearchText] = useState<string>('');
  const [searchInput, setSearchInput] = useState<string>('');
  const [page, setPage] = useState(1);
  const pageSize = 12;

  const fetchPrompts = useCallback(async () => {
    setIsLoading(true);
    setError('');
    try {
      const data = await promptApi.getPrompts({
        categoryId,
        searchText,
        page,
        pageSize
      });
      setPromptsPage(data);
    } catch (err) {
      setError('Failed to load prompts');
    } finally {
      setIsLoading(false);
    }
  }, [categoryId, searchText, page, pageSize]);

  useEffect(() => {
    fetchPrompts();
  }, [fetchPrompts]);

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    setSearchText(searchInput);
    setPage(1); // Reset page on new search
  };

  const handleCategorySelect = (id: string | undefined) => {
    setCategoryId(id);
    setPage(1);
  };

  return (
    <div className="prompt-library">
      <aside className="sidebar-container">
        <CategorySidebar 
          selectedCategoryId={categoryId} 
          onSelectCategory={handleCategorySelect} 
        />
      </aside>

      <div className="main-content-area">
        <div className="library-header">
          <h2>Prompt Library</h2>
          <div className="library-header-actions" style={{ display: 'flex', gap: '1rem', alignItems: 'center' }}>
            <form className="search-bar" onSubmit={handleSearch}>
              <input 
                type="text" 
                placeholder="Search prompts..." 
                value={searchInput}
                onChange={(e) => setSearchInput(e.target.value)}
                onKeyDown={(e) => {
                  if (e.key === 'Enter') {
                    e.preventDefault();
                    handleSearch(e as any);
                  }
                }}
              />
              <button type="submit" className="view-btn" style={{ margin: 0, height: '40px', padding: '0 1.25rem' }}>Search</button>
            </form>
            <button className="execute-btn" onClick={() => navigate('/prompts/new')} style={{ margin: 0, height: '40px', padding: '0 1.25rem' }}>New Prompt</button>
          </div>
        </div>

        {isLoading ? (
          <div className="loading-state">Loading prompts...</div>
        ) : error ? (
          <div className="error-state">{error}</div>
        ) : (
          <>
            {promptsPage?.items.length === 0 ? (
              <div className="empty-state">No prompts found matching your criteria.</div>
            ) : (
              <div className="prompt-grid">
                {(() => {
                  const totalLibraryExecutions = promptsPage?.items.reduce((sum, prompt) => sum + (prompt.executionCount || 0), 0) || 0;
                  return promptsPage?.items.map(prompt => (
                    <PromptCard key={prompt.id} prompt={prompt} totalLibraryExecutions={totalLibraryExecutions} />
                  ));
                })()}
              </div>
            )}

            {promptsPage && promptsPage.totalCount > 0 && (
              <div className="pagination">
                <button 
                  disabled={page === 1}
                  onClick={() => setPage(p => p - 1)}
                >
                  Previous
                </button>
                <span>Page {page}</span>
                <button 
                  disabled={page * pageSize >= promptsPage.totalCount}
                  onClick={() => setPage(p => p + 1)}
                >
                  Next
                </button>
              </div>
            )}
          </>
        )}
      </div>
    </div>
  );
};

export default PromptLibrary;
