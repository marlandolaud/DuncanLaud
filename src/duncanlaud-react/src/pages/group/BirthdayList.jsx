import defaultAvatar from '../../assets/default-avatar.svg';
import { personImageUrl } from '../../services/groupApi';

const DAY_NAMES = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
const MONTH_NAMES = ['January','February','March','April','May','June',
                     'July','August','September','October','November','December'];

export function calcDaysUntil(birthDateDisplay) {
  const parts = birthDateDisplay.split(' ');
  const month = MONTH_NAMES.indexOf(parts[0]);
  let day = parseInt(parts[1], 10);

  const today = new Date();
  today.setHours(0, 0, 0, 0);

  // Feb 29 in a non-leap year: treat as Feb 28
  if (month === 1 && day === 29) {
    const year = today.getFullYear();
    if (!(year % 4 === 0 && (year % 100 !== 0 || year % 400 === 0))) {
      day = 28;
    }
  }

  let birthday = new Date(today.getFullYear(), month, day);
  if (birthday < today) {
    const nextYear = today.getFullYear() + 1;
    if (month === 1 && day === 29 && !(nextYear % 4 === 0 && (nextYear % 100 !== 0 || nextYear % 400 === 0))) {
      day = 28;
    }
    birthday = new Date(nextYear, month, day);
  }

  return Math.round((birthday - today) / (1000 * 60 * 60 * 24));
}

function humanizeBirthday(daysUntil) {
  const today = new Date();
  const date = new Date(today);
  date.setDate(today.getDate() + daysUntil);

  const dayOfWeek = DAY_NAMES[date.getDay()];

  if (daysUntil === 0) return { primary: 'Today', subtitle: null };
  if (daysUntil === 1) return { primary: 'Tomorrow', subtitle: 'In 1 day' };
  if (daysUntil <= 6)  return { primary: `This ${dayOfWeek}`, subtitle: `In ${daysUntil} days` };
  if (daysUntil <= 13) return { primary: `Next ${dayOfWeek}`, subtitle: null };
  if (daysUntil <= 30) {
    const weeks = Math.floor(daysUntil / 7);
    return { primary: `In ${weeks} Weeks`, subtitle: null };
  }
  const months = date.getMonth() - today.getMonth() +
                 (date.getFullYear() - today.getFullYear()) * 12;
  const primary = months === 1 ? 'In a Month' : `In ${months} Months`;
  return { primary, subtitle: null };
}

function BirthdayCard({ person, groupId, imageVer }) {
  const daysUntil = calcDaysUntil(person.birthDateDisplay);
  const { primary, subtitle } = humanizeBirthday(daysUntil);

  const urgentClass = daysUntil <= 7 ? 'birthday-card--urgent' : '';
  const todayClass = daysUntil === 0 ? 'birthday-card__days--today' : '';
  const imgSrc = person.hasImage
    ? `${personImageUrl(groupId, person.personId)}?v=${imageVer}`
    : defaultAvatar;

  return (
    <div className={`birthday-card ${urgentClass}`}>
      <img
        className="birthday-card__photo"
        src={imgSrc}
        alt={person.displayName}
        onError={(e) => { e.currentTarget.src = defaultAvatar; }}
      />
      <div className="birthday-card__info">
        <span className="birthday-card__name">{person.displayName}</span>
        <span className="birthday-card__date">Born {person.birthDateDisplay}</span>
      </div>
      <div className="birthday-card__days-wrapper">
        <span className={`birthday-card__days ${todayClass}`}>{primary}</span>
        {subtitle && <span className="birthday-card__subtitle">{subtitle}</span>}
      </div>
    </div>
  );
}

export default function BirthdayList({ birthdays, groupId, imageVer }) {
  if (birthdays.length === 0) {
    return (
      <p className="birthday-list__empty">
        No upcoming birthdays in the next 60 days.
      </p>
    );
  }

  const sorted = [...birthdays].sort(
    (a, b) => calcDaysUntil(a.birthDateDisplay) - calcDaysUntil(b.birthDateDisplay)
  );

  return (
    <div className="birthday-list">
      {sorted.map((person) => (
        <BirthdayCard key={person.personId} person={person} groupId={groupId} imageVer={imageVer} />
      ))}
    </div>
  );
}
