/* UP */
/* Data Filters grouped by DataView in the correct order for migrations */
WITH CTE
AS (
    SELECT dv.NAME [DataView.Name]
        ,dv.[Guid] [DataView.Guid]
        ,dv.[Id] [DataView.Id]
        ,dvf.*
    FROM [DataViewFilter] [dvf]
    JOIN [DataView] dv ON dv.DataViewFilterId = dvf.Id
    
    UNION ALL
    
    SELECT pcte.[DataView.Name]
        ,pcte.[DataView.Guid]
        ,pcte.[DataView.Id]
        ,[a].*
    FROM [DataViewFilter] [a]
    INNER JOIN CTE pcte ON pcte.Id = [a].[ParentId]
    )
SELECT CONCAT (
        '// Create DataViewFilter for DataView: ' + dvf.[DataView.Name]
        ,'
/* NOTE to Developer. Review that the generated DataViewFilter.Selection for ' + et.NAME + ' will work on different databases */'
        ,'
Sql( @"
IF NOT EXISTS (SELECT * FROM DataViewFilter where [Guid] = '''
        ,dvf.[Guid]
        ,''') BEGIN    
    DECLARE
        @ParentDataViewFilterId int = (select Id from DataViewFilter where [Guid] = '''
        ,isnull(pdvf.[Guid], '00000000-0000-0000-0000-000000000000')
        ,'''),
        @DataViewFilterEntityTypeId int = (select Id from EntityType where [Guid] = '''
        ,isnull(et.[Guid], '00000000-0000-0000-0000-000000000000')
        ,''')

    INSERT INTO [DataViewFilter] (ExpressionType, ParentId, EntityTypeId, Selection, [Guid]) 
    values ('
        ,dvf.ExpressionType
        ,',@ParentDataViewFilterId,@DataViewFilterEntityTypeId,'''
        ,replace(dvf.[Selection], '"', '""')
        ,''','''
        ,[dvf].[Guid]
        ,''')'
        ,'
END
");'
        ) [DataviewFilter_UpMigration]
    ,dvf.[DataView.Name]
    ,dvf.[DataView.Guid]
    ,dvf.[DataView.Id]
    ,dvf.ExpressionType
    ,pdvf.[Guid] [ParentDataViewFilter.Guid]
    ,et.NAME [EntityType.Name]
    ,et.[Guid] [EntityType.Guid]
    ,dvf.Selection
FROM CTE [dvf]
LEFT JOIN [EntityType] [et] ON dvf.EntityTypeId = et.Id
LEFT JOIN [DataViewFilter] [pdvf] ON dvf.ParentId = pdvf.Id
ORDER BY [DataView.Id] DESC

/* DataViews*/
SELECT CONCAT (
        '// Create DataView: ' + dv.NAME
        ,'
Sql( @"
IF NOT EXISTS (SELECT * FROM DataView where [Guid] = '''
        ,dv.[Guid]
        ,''') BEGIN
DECLARE
    @categoryId int = (select top 1 [Id] from [Category] where [Guid] = '''
        ,isnull(c.[Guid], '00000000-0000-0000-0000-000000000000')
        ,'''),
    @entityTypeId  int = (select top 1 [Id] from [EntityType] where [Guid] = '''
        ,isnull(et.[Guid], '00000000-0000-0000-0000-000000000000')
        ,'''),
    @dataViewFilterId  int = (select top 1 [Id] from [DataViewFilter] where [Guid] = '''
        ,isnull(dvf.[Guid], '00000000-0000-0000-0000-000000000000')
        ,'''),
    @transformEntityTypeId  int = (select top 1 [Id] from [EntityType] where [Guid] = '''
        ,isnull(tet.[Guid], '00000000-0000-0000-0000-000000000000')
        ,''')

INSERT INTO [DataView] ([IsSystem], [Name], [Description], [CategoryId], [EntityTypeId], [DataViewFilterId], [TransformEntityTypeId], [Guid])
VALUES(0,'''
        ,dv.NAME
        ,''','''
        ,dv.[Description]
        ,''',@categoryId,@entityTypeId,@dataViewFilterId,@transformEntityTypeId,'''
        ,dv.[Guid]
        ,''')'
        ,'
END
");'
        ) [Dataview_UpMigration]
    ,dv.[Name]
    ,dv.[Description]
    ,c.[Guid] [Category.Guid]
    ,et.[Guid] [EntityType.Guid]
    ,dvf.[Guid] [DataViewFilter.Guid]
    ,tet.[Guid] [TransformEntityType.Guid]
    ,dv.[Guid]
FROM [DataView] [dv]
JOIN [Category] [c] ON dv.CategoryId = c.Id
JOIN [EntityType] [et] ON dv.EntityTypeId = et.Id
LEFT JOIN [EntityType] [tet] ON dv.TransformEntityTypeId = tet.Id
JOIN [DataViewFilter] [dvf] ON dv.DataViewFilterId = dvf.Id
ORDER BY dv.Id DESC;

/* Down for DataFilters (in Reverse Order) */
WITH CTE
AS (
    SELECT dv.NAME [DataView.Name]
        ,dv.[Guid] [DataView.Guid]
        ,dv.[Id] [DataView.Id]
        ,dvf.*
    FROM [DataViewFilter] [dvf]
    JOIN [DataView] dv ON dv.DataViewFilterId = dvf.Id
    
    UNION ALL
    
    SELECT pcte.[DataView.Name]
        ,pcte.[DataView.Guid]
        ,pcte.[DataView.Id]
        ,[a].*
    FROM [DataViewFilter] [a]
    INNER JOIN CTE pcte ON pcte.Id = [a].[ParentId]
    )
SELECT CONCAT (
        '// Delete DataViewFilter for DataView: ' + dvf.[DataView.Name]
        ,'
Sql( @"DELETE FROM DataViewFilter where [Guid] = '''
        ,dvf.[Guid]
        ,'''");'
        ) [DataviewFilter_Down_Migration]
FROM CTE [dvf]
LEFT JOIN [DataViewFilter] [pdvf] ON dvf.ParentId = pdvf.Id
ORDER BY [DataView.Id] DESC
    ,dvf.Id DESC

SELECT CONCAT (
        '// Delete DataView: ' + [Name]
        ,'
Sql( @"DELETE FROM DataView where [Guid] = '''
        ,[Guid]
        ,'''");'
        ) [Dataview_Down_Migration]
FROM [DataView]
ORDER BY [Id] DESC
