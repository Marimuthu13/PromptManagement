import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './features/auth/AuthContext';
import Login from './features/auth/Login';
import Register from './features/auth/Register';
import MainLayout from './components/layout/MainLayout';
import './App.css';

import PromptLibrary from './features/prompts/PromptLibrary';
import PromptEditor from './features/prompts/PromptEditor';
import PromptExecution from './features/prompts/PromptExecution';
import Templates from './features/templates/Templates';
import Dashboard from './features/analytics/Dashboard';

function App() {
  return (
    <Router>
      <AuthProvider>
        <Routes>
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Register />} />
          
          <Route element={<MainLayout />}>
            <Route path="/" element={<PromptLibrary />} />
            <Route path="/templates" element={<Templates />} />
            <Route path="/dashboard" element={<Dashboard />} />
            <Route path="/prompts/new" element={<PromptEditor />} />
            <Route path="/prompts/:id/edit" element={<PromptEditor />} />
            <Route path="/prompts/:id/execute" element={<PromptExecution />} />
          </Route>
          
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </AuthProvider>
    </Router>
  );
}

export default App;
