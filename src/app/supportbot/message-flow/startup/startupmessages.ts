import { Injectable } from '@angular/core';
import { IMessageFlowProvider } from '../../interfaces/imessageflowprovider';
import { TextMessage } from '../../models/message';
import { MessageGroup } from '../../models/message-group';
import { RegisterMessageFlowWithFactory } from '../message-flow.factory';
import { AuthService } from '../../../shared/services/auth.service';
import { ResourceType } from '../../../shared/models/portal';

@Injectable()
@RegisterMessageFlowWithFactory()
export class StartupMessages implements IMessageFlowProvider {

    constructor(private _authService: AuthService) {
    }

    public GetMessageFlowList(): MessageGroup[] {

        var messageGroupList: MessageGroup[] = [];

        let resourceName = this._authService.resourceType === ResourceType.Site ? 'App Service' : 'App Service Environment';
        let resourceNameSecond = this._authService.resourceType === ResourceType.Site ? 'Web App' : 'App Service Environment';

        var welcomeMessageGroup: MessageGroup = new MessageGroup('startup', [], () => 'main-menu');
        welcomeMessageGroup.messages.push(new TextMessage(`Hello! Welcome to ${resourceName} diagnostics! I’m here to help you diagnose and solve problems with your ${resourceNameSecond}.`));

        messageGroupList.push(welcomeMessageGroup);

        return messageGroupList;
    }
}