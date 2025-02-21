import React from 'react';
import { Link } from 'react-router-dom';

function HomePage() {
  return (
    <div>
      <h1>Welcome to Home Page</h1>
      <Link to="/login">Go to Login Page</Link>
    </div>
  );
}

export default HomePage;
