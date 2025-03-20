import React from 'react';
import ReactDOM from 'react-dom/client';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import './index.js';
import Home from './components/HomePage/Home.js';
import LoginPage from './components/LoginPage';
import reportWebVitals from './reportWebVitals.js';
import './styles/index.css';

const root = ReactDOM.createRoot(document.getElementById('root'));
root.render(
  <Router>
    <Routes>
      <Route path="/" element={<Home />} />
      <Route path="/login" element={<LoginPage />} />
    </Routes>
  </Router>
);

reportWebVitals();