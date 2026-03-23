import { useState, useEffect, useCallback } from 'react';
import { fetchGroup, createGroup, fetchBirthdays } from '../../services/groupApi';
import AddPersonForm from './AddPersonForm';
import BirthdayList from './BirthdayList';

const STATE = { LOADING: 'loading', NEW_GROUP: 'new_group', ADD_MEMBER: 'add_member', LANDING: 'landing', ERROR: 'error' };

export default function GroupLandingView({ groupId }) {
  const [view, setView] = useState(STATE.LOADING);
  const [group, setGroup] = useState(null);
  const [birthdays, setBirthdays] = useState([]);
  const [error, setError] = useState('');
  const [copied, setCopied] = useState(false);

  const loadBirthdays = useCallback(async () => {
    try {
      const data = await fetchBirthdays(groupId);
      setBirthdays(data);
    } catch {
      // Non-fatal — just show empty list
      setBirthdays([]);
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
          // Group not found — create it using stored name or prompt
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
            // No stored name — shouldn't happen via normal flow, but handle gracefully
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

  function copyLink() {
    const url = window.location.href;
    navigator.clipboard?.writeText(url).then(() => {
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    });
  }

  if (view === STATE.LOADING) {
    return (
      <div className="group-page group-page--loading" aria-live="polite" aria-label="Loading group">
        <div className="group-loading__spinner" />
        <p>Loading your group…</p>
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
          <button
            className="group-header__add-btn"
            onClick={() => setView(STATE.ADD_MEMBER)}
            type="button"
          >
            + Add Member
          </button>
        </div>
      </div>

      <section className="group-birthdays">
        <h2 className="group-birthdays__heading">Upcoming Birthdays (next 60 days)</h2>
        <BirthdayList birthdays={birthdays} groupId={groupId} />
      </section>
    </div>
  );
}
