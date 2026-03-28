import { useState, useEffect, useCallback } from 'react';
import { fetchGroup, createGroup, fetchBirthdays, fetchPersons } from '../../services/groupApi';
import AddPersonForm from './AddPersonForm';
import EditPersonForm from './EditPersonForm';
import BirthdayList from './BirthdayList';
import MemberList from './MemberList';

const STATE = {
  LOADING: 'loading',
  NEW_GROUP: 'new_group',
  ADD_MEMBER: 'add_member',
  EDIT_MEMBER: 'edit_member',
  VIEW_MEMBERS: 'view_members',
  LANDING: 'landing',
  ERROR: 'error',
};

export default function GroupLandingView({ groupId }) {
  const [view, setView] = useState(STATE.LOADING);
  const [group, setGroup] = useState(null);
  const [birthdays, setBirthdays] = useState([]);
  const [persons, setPersons] = useState([]);
  const [editPersonId, setEditPersonId] = useState(null);
  const [error, setError] = useState('');
  const [copied, setCopied] = useState(false);

  const loadBirthdays = useCallback(async () => {
    try {
      const data = await fetchBirthdays(groupId);
      setBirthdays(data);
    } catch {
      setBirthdays([]);
    }
  }, [groupId]);

  const loadPersons = useCallback(async () => {
    try {
      const data = await fetchPersons(groupId);
      setPersons(data);
    } catch {
      setPersons([]);
    }
  }, [groupId]);

  useEffect(() => {
    async function init() {
      try {
        const g = await fetchGroup(groupId);
        setGroup(g);
        await loadBirthdays();
        setView(STATE.LANDING);
      } catch (err) {
        if (err.status === 404) {
          const storedName = sessionStorage.getItem(`group_name_${groupId}`);
          if (storedName) {
            try {
              const g = await createGroup(groupId, storedName);
              sessionStorage.removeItem(`group_name_${groupId}`);
              setGroup(g);
              setView(STATE.NEW_GROUP);
            } catch (createErr) {
              setError(createErr.message || 'Could not create group.');
              setView(STATE.ERROR);
            }
          } else {
            setError('Group not found. Please create a new group from the Birthday Groups page.');
            setView(STATE.ERROR);
          }
        } else {
          setError(err.message || 'Could not load group.');
          setView(STATE.ERROR);
        }
      }
    }
    init();
  }, [groupId, loadBirthdays]);

  function handlePersonAdded() {
    loadBirthdays().then(() => setView(STATE.LANDING));
  }

  function handlePersonUpdated() {
    Promise.all([loadBirthdays(), loadPersons()]).then(() => {
      setEditPersonId(null);
      setView(STATE.VIEW_MEMBERS);
    });
  }

  function handleEditPerson(personId) {
    setEditPersonId(personId);
    setView(STATE.EDIT_MEMBER);
  }

  async function handleViewMembers() {
    await loadPersons();
    setView(STATE.VIEW_MEMBERS);
  }

  async function copyLink() {
    const url = window.location.href;
    try {
      if (navigator.clipboard) {
        await navigator.clipboard.writeText(url);
      } else {
        // Fallback for non-secure contexts
        const el = document.createElement('textarea');
        el.value = url;
        el.style.cssText = 'position:fixed;top:0;left:0;opacity:0';
        document.body.appendChild(el);
        el.focus();
        el.select();
        document.execCommand('copy');
        document.body.removeChild(el);
      }
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    } catch {
      // clipboard write failed silently
    }
  }

  if (view === STATE.LOADING) {
    return (
      <div className="group-page group-page--loading" aria-live="polite" aria-label="Loading group">
        <div className="group-loading__spinner" />
        <p>Loading your group...</p>
      </div>
    );
  }

  if (view === STATE.ERROR) {
    return (
      <div className="group-page">
        <div className="group-error">
          <p className="group-error__message">{error}</p>
          <a href="/mygroup" className="group-error__link">Go back</a>
        </div>
      </div>
    );
  }

  if (view === STATE.NEW_GROUP || view === STATE.ADD_MEMBER) {
    return (
      <div className="group-page">
        <div className="group-header">
          <h1 className="group-header__name">{group?.name}</h1>
          <div className="group-header__share">
            <span className="group-header__share-label">Share link:</span>
            <code className="group-header__share-url">/mygroup/{groupId}</code>
            <button className="group-header__copy-btn" onClick={copyLink} type="button">
              {copied ? 'Copied!' : 'Copy'}
            </button>
          </div>
        </div>
        <AddPersonForm
          groupId={groupId}
          isFirstMember={view === STATE.NEW_GROUP}
          onSuccess={handlePersonAdded}
          onCancel={view === STATE.ADD_MEMBER ? () => setView(STATE.LANDING) : undefined}
        />
      </div>
    );
  }

  if (view === STATE.EDIT_MEMBER && editPersonId) {
    return (
      <div className="group-page">
        <div className="group-header">
          <h1 className="group-header__name">{group?.name}</h1>
        </div>
        <EditPersonForm
          groupId={groupId}
          personId={editPersonId}
          onSuccess={handlePersonUpdated}
          onCancel={() => {
            setEditPersonId(null);
            setView(STATE.VIEW_MEMBERS);
          }}
        />
      </div>
    );
  }

  if (view === STATE.VIEW_MEMBERS) {
    return (
      <div className="group-page">
        <div className="group-header">
          <h1 className="group-header__name">{group?.name}</h1>
          <div className="group-header__actions">
            <button
              className="group-header__back-btn"
              onClick={() => setView(STATE.LANDING)}
              type="button"
            >
              Back to Birthdays
            </button>
          </div>
        </div>

        <section className="group-members">
          <h2 className="group-members__heading">All Members ({persons.length})</h2>
          <MemberList persons={persons} groupId={groupId} onEdit={handleEditPerson} />
        </section>
      </div>
    );
  }

  // LANDING view
  return (
    <div className="group-page">
      <div className="group-header">
        <h1 className="group-header__name">{group?.name}</h1>
        <div className="group-header__actions">
          <div className="group-header__share">
            <span className="group-header__share-label">Share link:</span>
            <code className="group-header__share-url">/mygroup/{groupId}</code>
            <button className="group-header__copy-btn" onClick={copyLink} type="button">
              {copied ? 'Copied!' : 'Copy'}
            </button>
          </div>
          <div className="group-header__btns">
            <button
              className="group-header__add-btn"
              onClick={() => setView(STATE.ADD_MEMBER)}
              type="button"
            >
              + Add Member
            </button>
            <button
              className="group-header__members-btn"
              onClick={handleViewMembers}
              type="button"
            >
              View All Members
            </button>
          </div>
        </div>
      </div>

      <section className="group-birthdays">
        <h2 className="group-birthdays__heading">Upcoming Birthdays (next 60 days)</h2>
        <BirthdayList birthdays={birthdays} groupId={groupId} />
      </section>
    </div>
  );
}
