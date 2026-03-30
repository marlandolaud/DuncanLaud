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
    (select count(1) from analytics.page_events pe where pe.SessionId = s.SessionId) AS page_views,
    (select count(1) from analytics.api_events ae where ae.SessionId = s.SessionId) AS api_calls
FROM analytics.sessions s
GROUP BY
    s.SessionId,
    s.IpPrefix,
    s.SessionStart,
    s.SessionEnd,
    s.EventCount,
    s.IsBot
ORDER BY s.SessionStart DESC;

select * from analytics.sessions
select * from analytics.page_events
select * from analytics.api_events