import defaultAvatar from '../../assets/default-avatar.svg';
import { personImageUrl } from '../../services/groupApi';

function BirthdayCard({ person, groupId }) {
  const daysLabel =
    person.daysUntil === 0
      ? 'Today!'
      : person.daysUntil === 1
      ? 'Tomorrow'
      : `in ${person.daysUntil} days`;

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
