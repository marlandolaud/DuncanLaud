import { useNavigate } from 'react-router-dom';
import { books } from '../data/books';
import LazyImage from './LazyImage';

export default function Sidebar() {
  const navigate = useNavigate();

  const handleBookClick = (urlSlug) => {
    window.scrollTo({ top: 0, behavior: 'smooth' });
    navigate(`/book/${urlSlug}`);
  };

  return (
    <aside className="sidebar" aria-label="Publications">
      <div className="sidebar__header">
        <h2 className="sidebar__title">Publications</h2>
        <div className="sidebar__divider" />
      </div>

      <ul className="sidebar__list">
        {books.map((book) => (
          <li key={book.urlSlug} className="sidebar__item">
            <span className="sidebar__book-name">{book.name}</span>
            <button
              className="sidebar__thumb-btn"
              onClick={() => handleBookClick(book.urlSlug)}
              aria-label={`View details for ${book.name}`}
            >
              <LazyImage
                src={book.thumbnailURL}
                alt={`Cover of ${book.name}`}
                className="sidebar__thumb"
                style={{ height: '200px', width: '130px' }}
              />
              <span className="sidebar__thumb-overlay">View Book</span>
            </button>

            {book.amazonId && (
              <a
                href={book.purchaseURL}
                target="_blank"
                rel="noopener noreferrer"
                className="sidebar__buy-btn"
                aria-label={`Buy ${book.name}`}
              >
                Buy Now
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" aria-hidden="true">
                  <path d="M18 13v6a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h6" />
                  <polyline points="15 3 21 3 21 9" />
                  <line x1="10" y1="14" x2="21" y2="3" />
                </svg>
              </a>
            )}
          </li>
        ))}
      </ul>
    </aside>
  );
}
