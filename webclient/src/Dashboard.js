import { useNavigate } from 'react-router-dom';

function Dashboard({ token, setToken }) {
    const navigate = useNavigate();


    const handleLogout = () => {
        localStorage.removeItem('jwt_token');
        setToken(null);
        navigate('/login'); 
    };

    return (
        <div className="protected-container">
            <p>Trang này yêu cầu token hợp lệ để duy trì phiên.</p>

            <button className="logout-button" onClick={handleLogout}>Đăng Xuất</button>
        </div>
    );
}

export default Dashboard;