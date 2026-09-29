import React, { useEffect, useState } from 'react';
import { promptApi } from '../../services/api/promptApi';
import type { TokenAnalyticsDto } from '../../services/types/prompt';
import './Dashboard.css';

const Dashboard: React.FC = () => {
  const [analytics, setAnalytics] = useState<TokenAnalyticsDto | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    const fetchAnalytics = async () => {
      try {
        const data = await promptApi.getAnalytics();
        setAnalytics(data);
      } catch (err) {
        setError('Failed to load analytics data.');
      } finally {
        setIsLoading(false);
      }
    };
    fetchAnalytics();
  }, []);

  if (isLoading) return <div className="dashboard-loading">Loading analytics...</div>;
  if (error) return <div className="dashboard-error">{error}</div>;
  if (!analytics) return null;

  return (
    <div className="dashboard-container">
      <div className="dashboard-header">
        <h2>Analytics Dashboard</h2>
      </div>

      <div className="metrics-row">
        <div className="metric-card">
          <div className="metric-title">Total System Tokens</div>
          <div className="metric-value">
            {analytics.totalSystemTokens.toLocaleString()}
          </div>
        </div>
      </div>

      <div className="dashboard-columns">
        <div className="dashboard-column">
          <h3>Top 5 Expensive Prompts</h3>
          <table className="analytics-table">
            <thead>
              <tr>
                <th>Prompt Title</th>
                <th className="text-right">Total Tokens</th>
              </tr>
            </thead>
            <tbody>
              {analytics.topPromptsByUsage.map((p) => (
                <tr key={p.promptId}>
                  <td>{p.title}</td>
                  <td className="text-right">{p.totalTokens.toLocaleString()}</td>
                </tr>
              ))}
              {analytics.topPromptsByUsage.length === 0 && (
                <tr>
                  <td colSpan={2}>No data available</td>
                </tr>
              )}
            </tbody>
          </table>
        </div>

        <div className="dashboard-column">
          <h3>AI API Request Stream</h3>
          <ul className="recent-executions-list">
            {analytics.recentExecutions.map((exec, idx) => {
              const getFeatureBadgeClass = (featureType: string) => {
                switch(featureType) {
                  case 'Vision Analysis': return 'badge-feature-purple';
                  case 'Prompt Doctor': return 'badge-feature-blue';
                  case 'Prompt Execution': return 'badge-feature-green';
                  case 'Variable Autofill': return 'badge-feature-amber';
                  default: return 'badge-feature-gray';
                }
              };
              
              const getStatusBadgeClass = (status: string) => {
                switch(status) {
                  case 'Success': return 'badge-status-success';
                  case 'Fallback': return 'badge-status-fallback';
                  case 'Failed': return 'badge-status-error';
                  default: return 'badge-status-gray';
                }
              };

              return (
                <li key={idx} className="execution-card">
                  <div className="execution-header-row">
                    <span className={`badge ${getFeatureBadgeClass(exec.featureType)}`}>
                      {exec.featureType}
                    </span>
                    <span className={`badge ${getStatusBadgeClass(exec.status)}`}>
                      {exec.status}
                    </span>
                  </div>
                  <div className="execution-title">{exec.displayTitle}</div>
                  <div className="execution-meta-grid">
                    <div className="meta-item">
                      <span className="meta-label">Model:</span>
                      <span className="meta-value badge-model">{exec.modelUsed}</span>
                    </div>
                    <div className="meta-item">
                      <span className="meta-label">Tokens:</span>
                      <span className="meta-value">{exec.totalTokens.toLocaleString()} total tokens</span>
                    </div>
                    <div className="meta-item">
                      <span className="meta-label">Latency:</span>
                      <span className="meta-value badge-latency">{exec.latencyMs} ms</span>
                    </div>
                    <div className="meta-item">
                      <span className="meta-label">Time:</span>
                      <span className="meta-value">{new Date(exec.timestamp).toLocaleString()}</span>
                    </div>
                  </div>
                </li>
              );
            })}
            {analytics.recentExecutions.length === 0 && (
              <li>No recent API activity</li>
            )}
          </ul>
        </div>
      </div>
    </div>
  );
};

export default Dashboard;
