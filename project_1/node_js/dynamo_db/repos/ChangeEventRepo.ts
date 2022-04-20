import config from '../../../config';
import { AwsConfig } from '../../../config/types';
import { ChangeEvent } from '../entities/ChangeEvent';
import { DynamoDbItem } from '../entities/DynamoDbItem';

import { DynamoDbRepoBase } from './DynamoDbRepoBase';

export class ChangeEventRepo extends DynamoDbRepoBase<ChangeEvent> {
  constructor() {
    const awsConfig = config.get('aws') as AwsConfig;

    super(awsConfig.dynamoDb.tableNames.changeEvents);
  }

  async get(instanceId: string, messageId: string): Promise<ChangeEvent> {
    return (
      await this.dynamoDbContext.get<DynamoDbItem<ChangeEvent>>({
        PK: `INSTANCE#${instanceId}`,
        SK: `MESSAGE#${messageId}`,
      })
    ).data;
  }

  protected transformToDynamoDbItem(item: ChangeEvent): DynamoDbItem<ChangeEvent> {
    return {
      PK: `INSTANCE#${}`,
      SK: `MESSAGE#${}`,
      GSI1PK: `STREAM#${}#YEAR#${}`,
      GSI1SK: `CHANGED#${}#CHANGETYPE#${}`,
      data: item,
    };
  }
}
