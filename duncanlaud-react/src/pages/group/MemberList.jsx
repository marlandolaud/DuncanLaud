import defaultAvatar from '../../assets/default-avatar.svg';
import { personImageUrl } from '../../services/groupApi';

function MemberCard({ person, groupId, onEdit }) {
  const imgSrc = person.hasImage ? personImageUrl(groupId, person.id) : defaultAvatar;
  const displayName = person.preferredName || `${person.firstName} ${person.lastName}`;

  return (
    <div className="member-card">
      <img
        className="member-card__photo"
        src={imgSrc}
        alt={displayName}
        onError={(e) => { e.currentTarget.src = defaultAvatar; }}
      />
      <div className="member-card__info">
        <span className="member-card__name">{displayName}</span>
        {person.preferredName && (
          <span className="member-card__full-name">{person.firstName} {person.lastName}</span>
        )}
        <span className="member-card__date">Born {person.birthDate}</span>
      </div>
      <button
        className="member-card__edit-btn"
        onClick={() => onEdit(person.id)}
        type="button"
        aria-label={`Edit ${displayName}`}
      >
        Edit
      </button>
    </div>
  );
}

export default function MemberList({ persons, groupId, onEdit }) {
  if (persons.length === 0) {
    return (
      <p className="member-list__empty">
        No members yet. Add someone to get started!
      </p>
    );
  }

  return (
    <div className="member-list">
      {persons.map((person) => (
        <MemberCard key={person.id} person={person} groupId={groupId} onEdit={onEdit} />
      ))}
    </div>
  );
}
