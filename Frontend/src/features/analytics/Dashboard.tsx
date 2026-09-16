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
          <h3>Recent Executions</h3>
          <ul className="recent-executions-list">
            {analytics.recentExecutions.map((exec, idx) => (
              <li key={idx} className="execution-item">
                <div className="execution-title">{exec.title}</div>
                <div className="execution-meta">
                  <span className="execution-time">{new Date(exec.timestamp).toLocaleString()}</span>
                  <span className="execution-tokens">{exec.totalTokens} tokens</span>
                </div>
              </li>
            ))}
            {analytics.recentExecutions.length === 0 && (
              <li>No recent executions</li>
            )}
          </ul>
        </div>
      </div>
    </div>
  );
};

export default Dashboard;
