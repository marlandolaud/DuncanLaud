import { author } from '../data/books';
import LazyImage from '../components/LazyImage';

export default function AboutPage() {
  return (
    <div className="page-content">
      <article className="article-card about-card">
        <h1 className="about-card__name">{author.name}</h1>
        <p className="about-card__pen-name">Writing as <em>{author.penName}</em></p>

        <div className="about-card__layout">
          <div className="about-card__image-col">
            <LazyImage
              src={author.imageURL}
              alt={`Portrait of ${author.name}`}
              style={{ height: '320px', width: '210px', borderRadius: '8px' }}
            />
          </div>

          <div className="about-card__bio-col">
            {author.bio.split('\n\n').map((para, i) => (
              <p key={i}>{para}</p>
            ))}

            <div className="about-card__contact">
              <h3>Get in Touch</h3>
              <a
                href={`mailto:${author.email}`}
                className="about-card__email-link"
                aria-label={`Email ${author.name}`}
              >
                <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" aria-hidden="true">
                  <rect x="2" y="4" width="20" height="16" rx="2" />
                  <path d="m22 7-8.97 5.7a1.94 1.94 0 0 1-2.06 0L2 7" />
                </svg>
                {author.email}
              </a>

              <a
                href={author.facebookURL}
                target="_blank"
                rel="noopener noreferrer"
                className="about-card__social-link"
                aria-label="Christine Duncan-Laud on Facebook"
              >
                <svg width="18" height="18" viewBox="0 0 24 24" fill="currentColor" aria-hidden="true">
                  <path d="M18 2h-3a5 5 0 0 0-5 5v3H7v4h3v8h4v-8h3l1-4h-4V7a1 1 0 0 1 1-1h3z" />
                </svg>
                Facebook
              </a>
            </div>
          </div>
        </div>
      </article>
    </div>
  );
}
