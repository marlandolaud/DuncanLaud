/*
delete from analytics.sessions
delete from analytics.page_events
delete from analytics.api_events
*/

SELECT
    s.SessionId,
    s.IpPrefix,
    s.SessionStart,
    s.SessionEnd,
    s.EventCount,
    s.IsBot,
    pe.page_views,
    ae.api_calls
FROM analytics.sessions s
CROSS APPLY (
    SELECT COUNT(1) AS page_views
    FROM analytics.page_events
    WHERE SessionId = s.SessionId
) pe
CROSS APPLY (
    SELECT COUNT(1) AS api_calls
    FROM analytics.api_events
    WHERE SessionId = s.SessionId
) ae
ORDER BY s.SessionStart DESC;