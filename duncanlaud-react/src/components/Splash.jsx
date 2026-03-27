import { useEffect, useRef } from 'react';

export default function Splash() {
  const splashRef = useRef(null);

  // Parallax scroll effect
  useEffect(() => {
    const el = splashRef.current;
    if (!el) return;

    const onScroll = () => {
      const offset = window.scrollY;
      el.style.backgroundPositionY = `calc(50% + ${(offset * 0.3).toFixed(2)}px)`;
    };

    window.addEventListener('scroll', onScroll, { passive: true });
    return () => window.removeEventListener('scroll', onScroll);
  }, []);

  return (
    <div className="splash" ref={splashRef} role="banner">
      <div className="splash__overlay" />
      <div className="splash__content">
        <p className="splash__eyebrow">Poetry &amp; Prose</p>
        <h1 className="splash__title">DuncanLaud.com</h1>
        <p className="splash__subtitle">
          Words that comfort, encourage, inspire, any challenge
        </p>
        <a href="#main-content" className="splash__cta">
          Explore the Collection
        </a>
      </div>
    </div>
  );
}
