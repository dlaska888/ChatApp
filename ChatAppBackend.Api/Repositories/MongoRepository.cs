using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Driver;
using WebService.Attributes;
using WebService.Models.Entities.Interfaces;
using WebService.Repositories.Interfaces;

namespace WebService.Repositories
{
    public sealed class MongoRepository<TDocument> : IMongoRepository<TDocument>
        where TDocument : IDocument
    {
        private readonly IMongoCollection<TDocument> _collection;

        public MongoRepository(IMongoDatabase database)
        {
            _collection = database.GetCollection<TDocument>(GetCollectionName(typeof(TDocument)));
        }

        private string GetCollectionName(Type documentType)
        {
            return ((BsonCollectionAttribute)documentType
                .GetCustomAttributes(typeof(BsonCollectionAttribute), true)
                .FirstOrDefault()!).CollectionName;
        }

        public IQueryable<TDocument> AsQueryable()
        {
            return _collection.AsQueryable();
        }

        public IEnumerable<TDocument> FilterBy(
            Expression<Func<TDocument, bool>> filterExpression)
        {
            return _collection.Find(filterExpression).ToEnumerable();
        }

        public IEnumerable<TProjected> FilterBy<TProjected>(
            Expression<Func<TDocument, bool>> filterExpression,
            Expression<Func<TDocument, TProjected>> projectionExpression)
        {
            return _collection.Find(filterExpression).Project(projectionExpression).ToEnumerable();
        }

        public TDocument FindOne(Expression<Func<TDocument, bool>> filterExpression)
        {
            return _collection.Find(filterExpression).FirstOrDefault();
        }

        public Task<TDocument> FindOneAsync(Expression<Func<TDocument, bool>> filterExpression)
        {
            return _collection.Find(filterExpression).FirstOrDefaultAsync();
        }

        public TDocument FindById(string id)
        {
            var objectId = new ObjectId(id);
            return _collection.Find(doc => doc.Id == objectId).FirstOrDefault();
        }

        public Task<TDocument> FindByIdAsync(string id)
        {
            var objectId = new ObjectId(id);
            return _collection.Find(doc => doc.Id == objectId).SingleOrDefaultAsync();
        }

        public TDocument InsertOne(TDocument document)
        {
            _collection.InsertOne(document);
            return document;
        }

        public async Task<TDocument> InsertOneAsync(TDocument document)
        {
            await _collection.InsertOneAsync(document);
            return document;
        }

        public void InsertMany(ICollection<TDocument> documents)
        {
            _collection.InsertMany(documents);
        }

        public async Task InsertManyAsync(ICollection<TDocument> documents)
        {
            await _collection.InsertManyAsync(documents);
        }

        public TDocument ReplaceOne(TDocument document)
        {
            var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, document.Id);
            _collection.FindOneAndReplace(filter, document);
            return document;
        }

        public async Task<TDocument> ReplaceOneAsync(TDocument document)
        {
            var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, document.Id);
            await _collection.FindOneAndReplaceAsync(filter, document);
            return document;
        }

        public bool DeleteOne(Expression<Func<TDocument, bool>> filterExpression)
        {
            var result = _collection.FindOneAndDelete(filterExpression);
            return result != null;
        }

        public async Task<bool> DeleteOneAsync(Expression<Func<TDocument, bool>> filterExpression)
        {
            var result = await _collection.FindOneAndDeleteAsync(filterExpression);
            return result != null;
        }

        public bool DeleteById(string id)
        {
            var objectId = new ObjectId(id);
            var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, objectId);
            var result = _collection.FindOneAndDelete(filter);
            return result != null;
        }

        public async Task<bool> DeleteByIdAsync(string id)
        {
            var objectId = new ObjectId(id);
            var result = await _collection.DeleteOneAsync(doc => doc.Id == objectId);
            return result.DeletedCount > 0;
        }

        public bool DeleteMany(Expression<Func<TDocument, bool>> filterExpression)
        {
            var result = _collection.DeleteMany(filterExpression);
            return result.DeletedCount > 0;
        }

        public async Task<bool> DeleteManyAsync(Expression<Func<TDocument, bool>> filterExpression)
        {
            var result = await _collection.DeleteManyAsync(filterExpression);
            return result.DeletedCount > 0;
        }
    }
}