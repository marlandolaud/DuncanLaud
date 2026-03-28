import { useState } from 'react';
import { useInView } from '../hooks/useLazyImage';

/**
 * LazyImage – only loads the real src once the element scrolls into view.
 * Shows a subtle shimmer placeholder while loading, then fades in.
 */
export default function LazyImage({ src, alt, className = '', style = {}, width, height }) {
  const [ref, inView] = useInView();
  const [loaded, setLoaded] = useState(false);

  return (
    <div
      ref={ref}
      className={`lazy-image-wrapper ${className}`}
      style={{ position: 'relative', overflow: 'hidden', ...style }}
    >
      {/* Shimmer placeholder shown until the real image loads */}
      {!loaded && <div className="lazy-shimmer" aria-hidden="true" />}

      {inView && (
        <img
          src={src}
          alt={alt}
          width={width}
          height={height}
          onLoad={() => setLoaded(true)}
          style={{
            display: 'block',
            width: '100%',
            height: '100%',
            objectFit: 'contain',
            opacity: loaded ? 1 : 0,
            transition: 'opacity 0.4s ease',
          }}
          decoding="async"
        />
      )}
    </div>
  );
}
