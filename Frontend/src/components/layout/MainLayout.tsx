import React from 'react';
import { Navigate, Outlet, Link } from 'react-router-dom';
import { useAuth } from '../../features/auth/AuthContext';
import './MainLayout.css';

const MainLayout: React.FC = () => {
  const { isAuthenticated, isLoading, user, logout } = useAuth();

  if (isLoading) {
    return <div className="layout-loading">Loading...</div>;
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  return (
    <div className="main-layout">
      <header className="main-header">
        <div className="header-brand">SmartPrompt</div>
        <nav className="header-nav" style={{ flexGrow: 1, marginLeft: '2rem', display: 'flex', gap: '1.5rem' }}>
          <Link to="/" style={{ textDecoration: 'none', color: '#4f46e5', fontWeight: 500 }}>Library</Link>
          <Link to="/templates" style={{ textDecoration: 'none', color: '#4f46e5', fontWeight: 500 }}>Templates</Link>
          <Link to="/dashboard" style={{ textDecoration: 'none', color: '#4f46e5', fontWeight: 500 }}>Dashboard</Link>
        </nav>
        <div className="header-actions">
          <span className="user-name">Welcome, {user?.name}</span>
          <button className="logout-button" onClick={logout}>Logout</button>
        </div>
      </header>
      <main className="main-content">
        <Outlet />
      </main>
    </div>
  );
};

export default MainLayout;
