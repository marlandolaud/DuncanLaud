import defaultAvatar from '../../assets/default-avatar.svg';
import { personImageUrl } from '../../services/groupApi';

function humanizeDays(days) {
  if (days === 0) return 'Today!';
  if (days === 1) return 'Tomorrow';
  if (days < 7) return `in ${days} days`;
  if (days < 14) return 'in 1 week';
  if (days < 21) return 'in 2 weeks';
  if (days < 28) return 'in 3 weeks';
  if (days < 45) return 'in 1 month';
  if (days < 60) return 'in 2 months';
  return `in ${days} days`;
}

function BirthdayCard({ person, groupId }) {
  const daysLabel = humanizeDays(person.daysUntil);

  const urgentClass = person.daysUntil <= 7 ? 'birthday-card--urgent' : '';
  const imgSrc = person.hasImage ? personImageUrl(groupId, person.personId) : defaultAvatar;

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
      <span className={`birthday-card__days ${person.daysUntil === 0 ? 'birthday-card__days--today' : ''}`}>
        {daysLabel}
      </span>
    </div>
  );
}

export default function BirthdayList({ birthdays, groupId }) {
  if (birthdays.length === 0) {
    return (
      <p className="birthday-list__empty">
        No upcoming birthdays in the next 60 days.
      </p>
    );
  }

  return (
    <div className="birthday-list">
      {birthdays.map((person) => (
        <BirthdayCard key={person.personId} person={person} groupId={groupId} />
      ))}
    </div>
  );
}
