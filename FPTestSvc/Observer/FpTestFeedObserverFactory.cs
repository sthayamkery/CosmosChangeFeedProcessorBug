using Microsoft.Azure.Documents.ChangeFeedProcessor.FeedProcessing;

namespace WK.FPTest.Observer
{
    public class FpTestFeedObserverFactory : IChangeFeedObserverFactory
    {
        private readonly IChangeFeedObserver _newObserver;
        /// <summary>
        /// Initializes a new instance of the <see cref="FpTestFeedObserverFactory" /> class.
        /// Saves input DocumentClient and DocumentCollectionInfo parameters to class fields
        /// </summary>
        public FpTestFeedObserverFactory(IChangeFeedObserver newObserver)
        {
            _newObserver = newObserver;
        }

        public IChangeFeedObserver CreateObserver()
        {
            return _newObserver;
        }
    }
}
