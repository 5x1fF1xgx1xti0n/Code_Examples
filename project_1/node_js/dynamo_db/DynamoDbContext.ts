import * as dynamoose from 'dynamoose';
import { Document } from 'dynamoose/dist/Document';
import { QueryResponse } from 'dynamoose/dist/DocumentRetriever';
import { ObjectType } from 'dynamoose/dist/General';
import { Schema, SchemaDefinition } from 'dynamoose/dist/Schema';

import config from '../../config';
import { AwsConfig } from '../../config/types';

import { DynamooseQueryTranslator } from './DynamooseQueryTranslator';
import { PagedResponse } from './models/PagedResponse';
import { PageRequest } from './models/PageRequest';

export class DynamoDbContext {
  private readonly dynamooseQueryTranslator: DynamooseQueryTranslator;
  private readonly tableName: string;
  private readonly schema: Schema;

  constructor(
    tableName: string,
    schemaDefinition: { [key: string]: unknown },
    schemaSettings: object,
  ) {
    this.configureDynamoDb(config.get('aws') as AwsConfig);
    this.dynamooseQueryTranslator = new DynamooseQueryTranslator();

    this.tableName = tableName;
    this.schema = new dynamoose.Schema(schemaDefinition as SchemaDefinition, schemaSettings);
  }

  async getAll<T>(request: PageRequest): Promise<PagedResponse<T>> {
    const itemModel = dynamoose.model<T & Document>(this.tableName, this.schema);
    const condition = this.dynamooseQueryTranslator.translateFilters(request.filters);

    try {
      let query = itemModel.query(condition);

      if (request.descending !== undefined) {
        query = query.sort(request.descending ? 'descending' : 'ascending');
      }

      let documentRetriever = query.startAt(request.lastKey as ObjectType);

      if (request.pageSize !== undefined) {
        documentRetriever = documentRetriever.limit(request.pageSize);
      }

      const result = (await documentRetriever.exec()) as QueryResponse<T>;

      const count: number = (await itemModel.query(condition).count().exec()).count;

      const response: PagedResponse<T> = {
        pageSize: request.pageSize,
        lastKey: result.lastKey,
        total: count,
        data: result as T[],
      };

      return response;
    } catch (error) {
      throw error;
    }
  }

  async get<T>(key: string | number | Record<string, string | number>): Promise<T> {
    const itemModel = dynamoose.model(this.tableName, this.schema);

    try {
      const item = await itemModel.get(key);

      return item.original() as T;
    } catch (error) {
      throw error;
    }
  }

  async create<T>(item: T): Promise<T> {
    const itemModel = dynamoose.model<T & Document>(this.tableName, this.schema);

    try {
      const resultItem = await itemModel.create(item);

      return resultItem as T;
    } catch (error) {
      throw error;
    }
  }

  async update<T>(item: T): Promise<T> {
    const itemModel = dynamoose.model(this.tableName, this.schema);

    try {
      const resultItem = await itemModel.update(item);

      return resultItem.original() as T;
    } catch (error) {
      throw error;
    }
  }

  async delete(key: string | number | Record<string, string | number>): Promise<void> {
    const itemModel = dynamoose.model(this.tableName, this.schema);

    try {
      await itemModel.delete(key);
    } catch (error) {
      throw error;
    }
  }

  private configureDynamoDb(awsConfig: AwsConfig) {
    const ddb = new dynamoose.aws.sdk.DynamoDB({
      region: awsConfig.dynamoDb.options.region,
      endpoint: awsConfig.dynamoDb.options.endpoint,
    });

    dynamoose.aws.ddb.set(ddb);
  }
}
