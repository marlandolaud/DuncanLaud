import { BrowserRouter, Routes, Route, Outlet, useLocation } from 'react-router-dom';
import { useEffect, useRef } from 'react';
import Navbar from './components/Navbar';
import Splash from './components/Splash';
import Sidebar from './components/Sidebar';
import Footer from './components/Footer';
import GroupLayout from './components/GroupLayout';
import HomePage from './pages/HomePage';
import AboutPage from './pages/AboutPage';
import BookPage from './pages/BookPage';
import MyGroupPage from './pages/group/MyGroupPage';

// Scroll to main content on route change, but not on initial load
function ScrollToTop() {
  const { pathname } = useLocation();
  const isFirstRender = useRef(true);
  useEffect(() => {
    if (isFirstRender.current) { isFirstRender.current = false; return; }
    const el = document.getElementById('main-content');
    window.scrollTo({ top: el ? el.offsetTop : 0, behavior: 'smooth' });
  }, [pathname]);
  return null;
}

function Layout() {
  return (
    <>
      <Navbar />
      <Splash />

      <main className="site-main" id="main-content">
        <div className="site-main__container">
          <div className="site-main__content-area">
            <Outlet />
          </div>

          <div className="site-main__sidebar-area">
            <Sidebar />
          </div>
        </div>
      </main>

      <Footer />
    </>
  );
}

export default function App() {
  return (
    <BrowserRouter>
      <ScrollToTop />
      <Routes>
        <Route element={<Layout />}>
          <Route path="/" element={<HomePage />} />
          <Route path="/about" element={<AboutPage />} />
          <Route path="/book/:bookId" element={<BookPage />} />
          <Route path="*" element={<HomePage />} />
        </Route>

        <Route element={<GroupLayout />}>
          <Route path="/mygroup" element={<MyGroupPage />} />
          <Route path="/mygroup/:groupId" element={<MyGroupPage />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}
