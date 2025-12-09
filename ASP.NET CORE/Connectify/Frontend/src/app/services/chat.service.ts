import { inject, Injectable, signal } from '@angular/core';
import { User } from '../models/user';
import { AuthService } from './auth.service';
import{HubConnection, HubConnectionBuilder, HubConnectionState} from '@microsoft/signalr';

@Injectable({
  providedIn: 'root'
})
export class ChatService {
private authService = inject(AuthService);
private hubUrl = 'http://localhost:5159/hubs/chat';
onlineUsers=signal<User[]>([]);
currentOpenedChat = signal<User|null>(null);

 private hubConnection?:HubConnection;

 startConnection(token:string, senderId?:string){
  this.hubConnection = new HubConnectionBuilder()
  .withUrl(`${this.hubUrl}?senderId=${senderId || ''}`,{
    accessTokenFactory:()=> token
  }).withAutomaticReconnect().build();

  this.hubConnection.start().then(()=>{
    console.log('Connection Started');
  }).catch((error)=>{
    console.log('Connection or login error',error);
 });

 this.hubConnection!.on('OnlineUsers',(user:User[])=>{
  console.log(user);
  this.onlineUsers.update(()=>
    user.filter(user=>user.userName !== this.authService.currentLoggedUser!.userName)
  )
 })
}

disConnectConnection(){
  if(this.hubConnection?.state=== HubConnectionState.Connected){
    this.hubConnection.stop().catch((error)=>console.log(error));
  }
}
status(userName:string):string{
  const currentChatUser=this.currentOpenedChat();
  if(!currentChatUser){
    return 'offline';
  }
  const onlineUser=this.onlineUsers().find(
    (user)=>user.userName===userName
  )
  return onlineUser?.isTyping?'Typing...':this.isUserOnline();
}

isUserOnline():string{
  let onlineUser=this.onlineUsers().find(
    (user)=>user.userName===this.currentOpenedChat()?.userName
  );
  return onlineUser?.isOnline?'Online':this.currentOpenedChat()!.userName;
}
}
