import { Link } from 'react-router-dom';
import { homeArticles } from '../data/books';
import LazyImage from '../components/LazyImage';

export default function HomePage() {
  return (
    <div className="page-content">
      {homeArticles.map((article, idx) => (
        <article key={article.id} className={`article-card ${idx === 0 ? 'article-card--intro' : ''}`}>
          {/* Intro card – portrait + welcome text */}
          {idx === 0 && (
            <div className="article-card__intro-layout">
              <div className="article-card__intro-image">
                <LazyImage
                  src={article.imgURL}
                  alt="C.A. Duncan-Laud"
                  style={{ height: '220px', width: '160px', borderRadius: '8px' }}
                />
              </div>
              <div className="article-card__intro-text">
                <h2 className="article-card__welcome">Welcome</h2>
                {article.body.split('\n\n').map((para, i) => (
                  <p key={i}>{para}</p>
                ))}
              </div>
            </div>
          )}

          {/* Feature cards – book announcements */}
          {idx > 0 && (
            <div className="article-card__feature-layout">
              <div className="article-card__feature-image">
                <LazyImage
                  src={article.imgURL}
                  alt={article.heading}
                  style={{ height: '280px', borderRadius: '8px' }}
                />
              </div>
              <div className="article-card__feature-text">
                {article.heading && <h2 className="article-card__heading">{article.heading}</h2>}
                {article.subheading && (
                  <p className="article-card__subheading">{article.subheading}</p>
                )}
                {article.body.split('\n\n').map((para, i) => (
                  <p key={i}>{para}</p>
                ))}
                {article.linkSlug && (
                  <Link to={`/book/${article.linkSlug}`} className="article-card__cta">
                    Learn More →
                  </Link>
                )}
              </div>
            </div>
          )}
        </article>
      ))}
    </div>
  );
}
