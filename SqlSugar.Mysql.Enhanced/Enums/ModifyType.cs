namespace Sqsugar.Mysql.Enhanced.Enums
{
    public enum ModifyType
    {
        /// <summary>
        /// 直接新增
        /// </summary>
        DirectInsert = 1,
        /// <summary>
        /// 根据唯一键(组合)更新该条数据
        /// </summary>
        UpdateExisted = 2,
        /// <summary>
        /// 根据唯一键(组合)忽略该条数据
        /// </summary>
        IgnoreExisted = 4,
        /// <summary>
        /// 根据唯一键(组合)先删除该条数据，再新增
        /// </summary>
        Replace = 8
    }
}
