import { useParams, useNavigate } from 'react-router-dom';
import './MyGroupPage.css';
import { useEffect } from 'react';
import { isValidGuid } from '../../utils/guid';
import CreateGroupView from './CreateGroupView';
import GroupLandingView from './GroupLandingView';

export default function MyGroupPage() {
  const { groupId } = useParams();
  const navigate = useNavigate();

  useEffect(() => {
    if (groupId && !isValidGuid(groupId)) {
      navigate('/', { replace: true });
    }
  }, [groupId, navigate]);

  if (!groupId) {
    return <CreateGroupView />;
  }

  if (!isValidGuid(groupId)) {
    return null;
  }

  return <GroupLandingView groupId={groupId} />;
}
