import { Outlet } from 'react-router-dom';
import Navbar from './Navbar';
import Footer from './Footer';

export default function GroupLayout() {
  return (
    <>
      <Navbar />
      <main className="group-main" id="main-content">
        <Outlet />
      </main>
      <Footer />
    </>
  );
}
