import themes from './celebrationThemes';

function randomBetween(min, max) {
  return Math.random() * (max - min) + min;
}

export default function CelebrationOverlay({ theme = 'birthday' }) {
  const config = themes[theme];
  if (!config) return null;

  const particles = [];
  let key = 0;

  for (const type of config.particles) {
    for (let i = 0; i < type.count; i++) {
      const style = {
        left: `${randomBetween(2, 98)}%`,
        animationDelay: `${randomBetween(0, 4).toFixed(2)}s`,
        animationDuration: `${randomBetween(5, 9).toFixed(2)}s`,
      };

      particles.push(
        <span
          key={key++}
          className={`celebration__particle celebration__particle--${type.className}`}
          style={style}
          aria-hidden="true"
        >
          {type.emoji}
        </span>
      );
    }
  }

  return (
    <div className="celebration" aria-hidden="true">
      {particles}
    </div>
  );
}
