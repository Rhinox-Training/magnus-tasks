using Rhinox.Vortex;
using Rhinox.Vortex.File;

namespace Rhinox.Magnus.Tasks
{
    public class TagDataTable : DataTable<TagData>
    {
        protected override string _tableName => "tagdata";
        
        protected override int GetID(TagData dataObject)
        {
            return dataObject.ID;
        }

        protected override TagData SetID(TagData dto, int id)
        {
            dto.ID = id;
            return dto;
        }
    }
}