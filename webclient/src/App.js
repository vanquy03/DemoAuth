import React, { useState, useEffect } from 'react';
import './App.css';
import { BrowserRouter as Router, Routes, Route, Navigate, useNavigate } from 'react-router-dom';
import Dashboard from './Dashboard';

// Cấu hình API
const BASE_URL = 'http://localhost:5280/api/Auth';

// COMPONENT CHÍNH (Xử lý Đăng nhập/Đăng ký) 
function AuthForm({ isLogin, setToken, setMessage, navigate }) {
    const [userName, setUserName] = useState('');
    const [password, setPassword] = useState('');
    const [role, setRole] = useState('EndUser');

    const handleAuth = async (endpoint, payload) => {
        setMessage('Đang xử lý...');
        try {
            const response = await fetch(`${BASE_URL}/${endpoint}`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload),
            });
            const data = await response.json();
            
            if (response.ok) {
                if (endpoint === 'login') {
                    localStorage.setItem('jwt_token', data.token);
                    console.log('Received Token:', data); // Debug: Kiểm tra token nhận được
                    setToken(data.token);
                    setMessage(data.message || 'Đăng nhập thành công!'); 
                    navigate('/dashboard');
                } else {
                    setMessage(data.message || 'Đăng ký thành công! Vui lòng đăng nhập.'); 
                    navigate('/login'); 
                }
            } else {
                const errorMsg = data.message || data.title || 'Lỗi không xác định.';
                setMessage(`Thất bại: ${errorMsg}`);
            }
        } catch (error) {
            setMessage(`Lỗi kết nối: ${error.message}`);
        }
    };

    const handleSubmit = (e) => {
        e.preventDefault();
        if (isLogin) {
            handleAuth('login', { userName, password });
        } else {
            handleAuth('register', { userName, password, roleName: role });
        }
    };

    return (
        <div className="form-container">
            <h2>{isLogin ? 'Đăng Nhập' : 'Đăng Ký'}</h2>
            <form onSubmit={handleSubmit}>
                <input type="text" placeholder="Tên đăng nhập" value={userName} onChange={(e) => setUserName(e.target.value)} required />
                <input type="password" placeholder="Mật khẩu" value={password} onChange={(e) => setPassword(e.target.value)} required />
                {!isLogin && (
                    <select value={role} onChange={(e) => setRole(e.target.value)}>
                        <option value="EndUser">EndUser</option>
                        <option value="Admin">Admin</option>
                    </select>
                )}
                <button type="submit">{isLogin ? 'Đăng Nhập' : 'Đăng Ký'}</button>
            </form>
            <p>
                {isLogin ? 'Chưa có tài khoản?' : 'Đã có tài khoản?'} 
                <button className="link-button" onClick={() => navigate(isLogin ? '/register' : '/login')}>
                    {isLogin ? 'Đăng ký ngay' : 'Đăng nhập'}
                </button>
            </p>
        </div>
    );
}

function AppContent() {
    const [token, setToken] = useState(localStorage.getItem('jwt_token'));
    const [message, setMessage] = useState('');
    const navigate = useNavigate();

    const ProtectedRoute = ({ children }) => {
        if (!token) {
            // Nếu không có token, chuyển hướng về trang đăng nhập
            setMessage('Vui lòng đăng nhập để truy cập trang này.');
            return <Navigate to="/login" replace />;
        }
        return children;
    };

    return (
        <div className="app-container">
            <div className="message-box">Trạng thái Client: <strong>{message}</strong></div>
            
            <Routes>
                <Route path="/login" element={<AuthForm isLogin={true} setToken={setToken} setMessage={setMessage} navigate={navigate} />} />
                <Route path="/register" element={<AuthForm isLogin={false} setToken={setToken} setMessage={setMessage} navigate={navigate} />} />
                <Route 
                    path="/dashboard" 
                    element={
                        <ProtectedRoute>
                            <Dashboard token={token} setToken={setToken} />
                        </ProtectedRoute>
                    } 
                />
                <Route path="*" element={<Navigate to="/login" replace />} />
            </Routes>
        </div>
    );
}

function App() {
    return (
        <Router>
            <AppContent />
        </Router>
    );
}

export default App;