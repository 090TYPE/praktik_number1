-- 1) ������� ������ ��������� � ������� 10 ����� ����� ����� ������: ������������, ��������, ������, ���������� ����� � ������� �� ��� ������� �� �������
-----------------------------------------------------------------------------------------------------------------------------------------------------------
WITH TaskCalls AS (
    SELECT 
        c.dttm AS call_dttm,
        t.id AS task_id,
        t.plugin_uid,
        u.fio,
        o.tag AS direction
    FROM 
        [zad2].[dbo].[calls] c
    INNER JOIN
        [zad2].[dbo].[tasks] t ON c.task_id = t.id
    INNER JOIN
        [zad2].[dbo].[users] u ON c.user_id = u.id
    INNER JOIN
        [zad2].[dbo].[otdels] o ON u.otdel_id = o.id
    WHERE 
        dttm BETWEEN c.dttm AND DATEADD(MINUTE, 10, c.dttm)
)
SELECT 
    fio,
    direction,
    plugin_uid,
    COUNT(task_id) AS task_count,
    COUNT(CASE WHEN task_id IS NOT NULL THEN 1 END) AS call_related_tasks
FROM 
    TaskCalls
GROUP BY
    fio, direction, plugin_uid;
-----------------------------------------------------------------------------------------------------------------------------------------------------------
-- 2) ������ ������������� �� ���� �������, ������� �� �� ����� � ������������ ������� ����� ���� ������ �� ���������� �������.
-----------------------------------------------------------------------------------------------------------------------------------------------------------
WITH TaskCalls AS (
    SELECT 
        c.dttm AS call_dttm,
        t.id AS task_id,
        t.plugin_uid,
        u.fio,
        o.tag AS direction,
        DATEPART(YEAR, c.dttm) AS call_year,
        DATEPART(MONTH, c.dttm) AS call_month
    FROM 
        [zad2].[dbo].[calls] c
    INNER JOIN
        [zad2].[dbo].[tasks] t ON c.task_id = t.id
    INNER JOIN
        [zad2].[dbo].[users] u ON c.user_id = u.id
    INNER JOIN
        [zad2].[dbo].[otdels] o ON u.otdel_id = o.id
    WHERE 
        -- ����� �������� ������� ��� ������� �� ������� �������
        DATEPART(YEAR, c.dttm) = 2023
        AND DATEPART(MONTH, c.dttm) = 01
)
SELECT 
    fio,
    direction,
    plugin_uid,
    COUNT(task_id) AS task_count,
    COUNT(CASE WHEN task_id IS NOT NULL THEN 1 END) AS call_related_tasks
FROM 
    TaskCalls
GROUP BY
    fio, direction, plugin_uid;
-----------------------------------------------------------------------------------------------------------------------------------------------------------