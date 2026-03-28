import { useParams, Link } from 'react-router-dom';
import { books } from '../data/books';
import LazyImage from '../components/LazyImage';

function formatDate(dateStr) {
  const d = new Date(dateStr + 'T00:00:00');
  return d.toLocaleDateString('en-US', { month: 'long', year: 'numeric' });
}

export default function BookPage() {
  const { bookId } = useParams();
  const book = books.find((b) => b.urlSlug === bookId);

  if (!book) {
    return (
      <div className="page-content">
        <div className="not-found-card">
          <h1>Book Not Found</h1>
          <p>Sorry, we couldn't find that book.</p>
          <Link to="/" className="article-card__cta">← Back to Home</Link>
        </div>
      </div>
    );
  }

  const hasDetails = parseInt(book.isbn10, 10) > 0;

  return (
    <div className="page-content">
      <article className="article-card book-detail-card">
        <h1 className="book-detail-card__title">{book.name}</h1>
        <p className="book-detail-card__author">by {book.author}</p>

        {/* Images row */}
        <div className="book-detail-card__images">
          <div className="book-detail-card__image-wrap">
            <LazyImage
              src={book.bookImageURL}
              alt={`Cover of ${book.name}`}
              style={{ height: '340px', borderRadius: '8px' }}
            />
          </div>
          {book.bookImageURL2 && (
            <div className="book-detail-card__image-wrap">
              <LazyImage
                src={book.bookImageURL2}
                alt={`Interior spread of ${book.name}`}
                style={{ height: '340px', borderRadius: '8px' }}
              />
            </div>
          )}
        </div>

        {/* Description */}
        {hasDetails && (
          <div className="book-detail-card__description">
            <h2 className="book-detail-card__desc-heading">{book.descriptionHeading}</h2>
            {book.descriptionBody.split('\n\n').map((para, i) => (
              <p key={i}>{para}</p>
            ))}
          </div>
        )}

        {/* Meta + Buy */}
        {hasDetails && (
          <div className="book-detail-card__meta">
            <div className="book-detail-card__meta-grid">
              <span className="book-detail-card__meta-label">Author</span>
              <span className="book-detail-card__meta-value">{book.author}</span>

              <span className="book-detail-card__meta-label">Published</span>
              <span className="book-detail-card__meta-value">{formatDate(book.publishDate)}</span>

              {book.suggestedRetailPriceUSD?.length > 0 && (
                <>
                  <span className="book-detail-card__meta-label">Price</span>
                  <span className="book-detail-card__meta-value">
                    {book.suggestedRetailPriceUSD.map((p) => `$${p}`).join(' / ')}
                  </span>
                </>
              )}

              {book.isbn13 && parseInt(book.isbn13, 10) > 0 && (
                <>
                  <span className="book-detail-card__meta-label">ISBN-13</span>
                  <span className="book-detail-card__meta-value">{book.isbn13}</span>
                </>
              )}

              {book.pages > 0 && (
                <>
                  <span className="book-detail-card__meta-label">Pages</span>
                  <span className="book-detail-card__meta-value">{book.pages}</span>
                </>
              )}

              {book.language && (
                <>
                  <span className="book-detail-card__meta-label">Language</span>
                  <span className="book-detail-card__meta-value">{book.language}</span>
                </>
              )}
            </div>

            {book.amazonId && (
              <a
                href={book.purchaseURL}
                target="_blank"
                rel="noopener noreferrer"
                className="book-detail-card__buy-btn"
                aria-label={`Purchase ${book.name}`}
              >
                Purchase This Book
                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" aria-hidden="true">
                  <path d="M18 13v6a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h6" />
                  <polyline points="15 3 21 3 21 9" />
                  <line x1="10" y1="14" x2="21" y2="3" />
                </svg>
              </a>
            )}
          </div>
        )}

        <div className="book-detail-card__back">
          <Link to="/" className="article-card__cta article-card__cta--outline">
            ← Back to All Books
          </Link>
        </div>
      </article>
    </div>
  );
}
