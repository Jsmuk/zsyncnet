using System.Dynamic;

namespace zsyncnet.Internal
{
    public class SyncOperation
    {
        public SyncOperation(BlockSum remoteBlock, BlockSum localBlock)
        {
            RemoteBlock = remoteBlock;
            LocalBlock = localBlock;
        }

        public BlockSum RemoteBlock { get; set; }
        public BlockSum LocalBlock { get; set; }
    }
}