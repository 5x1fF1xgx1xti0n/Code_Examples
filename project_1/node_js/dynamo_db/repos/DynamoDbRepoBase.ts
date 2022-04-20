import { DynamoDbContext } from '../DynamoDbContext';
import { DynamoDbItem } from '../entities/DynamoDbItem';
import { PagedResponse } from '../models/PagedResponse';
import { PageRequest } from '../models/PageRequest';

export abstract class DynamoDbRepoBase<T> {
  protected readonly dynamoDbContext: DynamoDbContext;

  constructor(tableName: string) {
    const schemaDefinition = {
      PK: {
        type: String,
        hashKey: true,
      },
      SK: {
        type: String,
        rangeKey: true,
      },
      GSI1PK: String,
      GSI1SK: String,
      GSI2PK: String,
      GSI2SK: String,
      GSI3PK: String,
      GSI3SK: String,
      data: Object,
    };

    const schemaSettings = {
      // allowing storing the property 'data' with infinitely nested level unknown properties
      saveUnknown: ['data.**'],
    };

    this.dynamoDbContext = new DynamoDbContext(tableName, schemaDefinition, schemaSettings);
  }

  async getAll(request: PageRequest): Promise<PagedResponse<T>> {
    const response = await this.dynamoDbContext.getAll<DynamoDbItem<T>>(request);

    return { ...response, data: response.data.map((i) => i.data) };
  }

  async create(item: T): Promise<T> {
    const dynamoDbItem = this.transformToDynamoDbItem(item);

    return (await this.dynamoDbContext.create<DynamoDbItem<T>>(dynamoDbItem)).data;
  }

  async update(item: T): Promise<T> {
    const dynamoDbItem = this.transformToDynamoDbItem(item);

    return (await this.dynamoDbContext.update<DynamoDbItem<T>>(dynamoDbItem)).data;
  }

  async delete(key: string | number | Record<string, string | number>): Promise<void> {
    await this.dynamoDbContext.delete(key);
  }

  protected abstract transformToDynamoDbItem(item: T): DynamoDbItem<T>;
}
