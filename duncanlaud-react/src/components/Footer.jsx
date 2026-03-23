import { Link } from 'react-router-dom';

export default function Footer() {
  const year = new Date().getFullYear();

  return (
    <footer className="footer">
      <div className="footer__inner">
        <div className="footer__links">
          <Link to="/" className="footer__link">Home</Link>
          <Link to="/about" className="footer__link">About</Link>
          <a href="https://penthumb.com" className="footer__link" target="_blank" rel="noopener noreferrer">Store</a>
          <a href="mailto:christine@duncanlaud.com" className="footer__link">Contact</a>
          <Link to="/mygroup" className="footer__link">Birthday Groups</Link>
        </div>
        <p className="footer__copy">
          &copy; {year} C.A. Duncan-Laud. All rights reserved.
        </p>
      </div>
    </footer>
  );
}
