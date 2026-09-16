import React from 'react';
import { Link } from 'react-router-dom';
import type { PromptDto } from '../../services/types/prompt';

interface PromptCardProps {
  prompt: PromptDto;
  totalLibraryExecutions: number;
}

const PromptCard: React.FC<PromptCardProps> = ({ prompt, totalLibraryExecutions }) => {
  const usagePercentage = totalLibraryExecutions > 0 
    ? Math.round(((prompt.executionCount || 0) / totalLibraryExecutions) * 100) 
    : 0;

  return (
    <div className={`prompt-card ${usagePercentage > 50 ? 'most-used' : ''}`}>
      <div className="prompt-card-header">
        <h4>{prompt.title} {usagePercentage > 50 && '🔥'}</h4>
      </div>
      <div className="prompt-card-body">
        <p>{prompt.description || 'No description provided.'}</p>
        {prompt.variables.length > 0 && (
          <div className="prompt-variables">
            <span className="variable-label">Variables:</span>
            {prompt.variables.map(v => (
              <span key={v.id} className="variable-tag">{v.name}</span>
            ))}
          </div>
        )}
        <div className="usage-metric" style={{ marginTop: '10px', fontSize: '0.85rem', color: '#6b7280' }}>
          ⚡ {prompt.executionCount || 0} uses ({usagePercentage}%)
        </div>
      </div>
      <div className="card-actions">
        <Link to={`/prompts/${prompt.id}/edit`} className="view-btn">Edit</Link>
        <Link to={`/prompts/${prompt.id}/execute`} className="execute-btn">Execute</Link>
      </div>
    </div>
  );
};

export default PromptCard;
