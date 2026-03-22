import { useState, useEffect, useRef } from 'react';

/**
 * Hook that returns whether an element has entered the viewport.
 * Used for efficient lazy image loading via IntersectionObserver.
 */
export function useInView(options = {}) {
  const ref = useRef(null);
  const [inView, setInView] = useState(false);

  useEffect(() => {
    const el = ref.current;
    if (!el) return;

    // If IntersectionObserver isn't supported, load immediately
    if (!('IntersectionObserver' in window)) {
      setInView(true);
      return;
    }

    const observer = new IntersectionObserver(
      ([entry]) => {
        if (entry.isIntersecting) {
          setInView(true);
          observer.disconnect(); // load once, then stop observing
        }
      },
      {
        rootMargin: '200px 0px', // start loading 200px before entering viewport
        threshold: 0,
        ...options,
      }
    );

    observer.observe(el);
    return () => observer.disconnect();
  }, []);

  return [ref, inView];
}
