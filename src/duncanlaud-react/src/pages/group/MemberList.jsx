import defaultAvatar from '../../assets/default-avatar.svg';
import { personImageUrl } from '../../services/groupApi';

function formatBirthDate(dateStr) {
  if (!dateStr) return '';
  const [year, month, day] = dateStr.split('-').map(Number);
  return new Date(year, month - 1, day).toLocaleDateString('en-US', { month: 'long', day: 'numeric' });
}

const PencilIcon = () => (
  <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" width="15" height="15" aria-hidden="true">
    <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7" />
    <path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z" />
  </svg>
);

const TrashIcon = () => (
  <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" width="15" height="15" aria-hidden="true">
    <polyline points="3 6 5 6 21 6" />
    <path d="M19 6l-1 14a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2L5 6" />
    <path d="M10 11v6" />
    <path d="M14 11v6" />
    <path d="M9 6V4a1 1 0 0 1 1-1h4a1 1 0 0 1 1 1v2" />
  </svg>
);

function MemberCard({ person, groupId, onEdit, onDelete, imageVer }) {
  const imgSrc = person.hasImage
    ? `${personImageUrl(groupId, person.id)}?v=${imageVer}`
    : defaultAvatar;
  const displayName = person.preferredName || `${person.firstName} ${person.lastName}`;

  function handleDelete() {
    if (window.confirm(`Remove ${displayName} from the group?`)) {
      onDelete(person.id);
    }
  }

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
        <span className="member-card__date">Born {formatBirthDate(person.birthDate)}</span>
      </div>
      <div className="member-card__actions">
        <button
          className="member-card__edit-btn"
          onClick={() => onEdit(person.id)}
          type="button"
          aria-label={`Edit ${displayName}`}
        >
          <PencilIcon />
        </button>
        <button
          className="member-card__delete-btn"
          onClick={handleDelete}
          type="button"
          aria-label={`Delete ${displayName}`}
        >
          <TrashIcon />
        </button>
      </div>
    </div>
  );
}

export default function MemberList({ persons, groupId, onEdit, onDelete, imageVer }) {
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
        <MemberCard
          key={person.id}
          person={person}
          groupId={groupId}
          onEdit={onEdit}
          onDelete={onDelete}
          imageVer={imageVer}
        />
      ))}
    </div>
  );
}
