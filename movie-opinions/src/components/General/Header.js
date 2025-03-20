import '../../styles/General/Header.css'
import '../../styles/General/Button.css'
import '../../styles/General/Fonts.css'
import React, { useEffect, useState } from "react";

const Header = () => {
    const[user, setUser] = useState(null);

    useEffect(() =>{
        const storedUser = localStorage.getItem("user");

        if(storedUser){
            setUser(JSON.parse(storedUser));
        }
    }, []);

    const handleLogout = () => {
        localStorage.removeItem("user");
        setUser(null);
    };

    const [menuOpen, setMenuOpen] = useState(false);
    const [loginOpen, setLoginOpen] = useState(false);

    return (
        <header className='header'>
            {/* Гамбургер-іконка */}
            <button className="header__menu-mobile" onClick={() => {setMenuOpen(!menuOpen); setLoginOpen(false)}}>
                ☰
            </button>

            {/* Логотип */}
            <span className='header__logo'>Movie Opinions</span>

            {/* Навігація */}
            <nav className={`header__nav ${menuOpen ? "open" : ""}`}>
                <ul className='header__menu'>
                    <li><a href='#'>Головна</a></li>
                    <li><a href='#'>Фільми</a></li>
                    <li><a href='#'>Залишити відгук</a></li>
                    <li><a href='#'>Контакти</a></li>
                </ul>
            </nav>

            {/* Гамбургер-іконка */}
            <button className="header__menu-Login-mobile" onClick={() => {setLoginOpen(!loginOpen); setMenuOpen(false)}}>
                <img src='/Image/Login_icon.png' className='header__img'></img>
            </button>

            {/* Авторизація */}
            <div className={`header__auth ${loginOpen ? "open" : ""}`}>
                {user ? (
                    <>
                        <span className="header__username">Вітаю, {user.name}!</span>
                        <button className="button button--logout" onClick={handleLogout}>Вийти</button>
                    </>
                ) : (
                    <>
                        <button className="button button--login">Увійти</button>
                        <button className="button button--register">Зареєструватися</button>
                    </>
                )}
            </div>
        </header>
    );
}

export default Header;