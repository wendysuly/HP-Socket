﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using HPSocketCS.SDK;

namespace HPSocketCS
{
    public class TcpClient
    {
        protected IntPtr _pClient = IntPtr.Zero;

        protected IntPtr pClient
        {
            get
            {
                //if (_pClient == IntPtr.Zero)
                //{
                //    throw new Exception("pClient == 0");
                //}

                return _pClient;
            }

            set
            {
                _pClient = value;
            }
        }


        protected IntPtr pListener = IntPtr.Zero;

        protected HPSocketSdk.HP_FN_OnConnect OnConnectCallback;
        protected HPSocketSdk.HP_FN_OnSend OnSendCallback;
        protected HPSocketSdk.HP_FN_OnPrepareConnect OnPrepareConnectCallback;
        protected HPSocketSdk.HP_FN_OnReceive OnReceiveCallback;
        protected HPSocketSdk.HP_FN_OnClose OnCloseCallback;
        protected HPSocketSdk.HP_FN_OnError OnErrorCallback;

        protected bool IsSetCallback = false;
        protected bool IsCreate = false;

        public TcpClient()
        {
            CreateListener();
        }

        ~TcpClient()
        {
            //if (HasStarted() == true)
            //{
            //    Stop();
            //}

            Destroy();
        }

        /// <summary>
        /// 创建socket监听&服务组件
        /// </summary>
        /// <param name="isUseDefaultCallback">是否使用tcpserver类默认回调函数</param>
        /// <returns></returns>
        public virtual bool CreateListener()
        {
            if (IsCreate == true || pListener != IntPtr.Zero || pClient != IntPtr.Zero)
            {
                return false;
            }

            pListener = HPSocketSdk.Create_HP_TcpClientListener();
            if (pListener == IntPtr.Zero)
            {
                return false;
            }

            pClient = HPSocketSdk.Create_HP_TcpClient(pListener);
            if (pClient == IntPtr.Zero)
            {
                return false;
            }

            IsCreate = true;

            return true;
        }

        /// <summary>
        /// 释放TcpServer和TcpServerListener
        /// </summary>
        public virtual void Destroy()
        {
            Stop();

            if (pClient != IntPtr.Zero)
            {
                HPSocketSdk.Destroy_HP_TcpClient(pClient);
                pClient = IntPtr.Zero;
            }
            if (pListener != IntPtr.Zero)
            {
                HPSocketSdk.Destroy_HP_TcpClientListener(pListener);
                pListener = IntPtr.Zero;
            }

            IsCreate = false;
        }

        /// <summary>
        /// 启动通讯组件并连接到服务器
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <param name="async">是否异步</param>
        /// <returns></returns>
        public bool Start(string address, ushort port, bool async = true)
        {
            if (IsSetCallback == false)
            {
               // throw new Exception("请在调用Start方法前先调用SetCallback()方法");
            }

            if (HasStarted() == true)
            {
                return false;
            }

            return HPSocketSdk.HP_Client_Start(pClient, address, port, async);
        }

        /// <summary>
        /// 停止通讯组件
        /// </summary>
        /// <returns></returns>
        public bool Stop()
        {
            if (HasStarted() == false)
            {
                return false;
            }
            return HPSocketSdk.HP_Client_Stop(pClient);
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="connId"></param>
        /// <param name="bytes"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public bool Send(uint connId, byte[] bytes, int size)
        {
            return HPSocketSdk.HP_Client_Send(pClient, connId, bytes, size);
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="connId"></param>
        /// <param name="bufferPtr"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public bool Send(uint connId, IntPtr bufferPtr, int size)
        {
            return HPSocketSdk.HP_Client_Send(pClient, connId, bufferPtr, size);
        }

        /// <summary>
        /// 获取错误码
        /// </summary>
        /// <returns></returns>
        public En_HP_SocketError GetlastError()
        {
            return HPSocketSdk.HP_Client_GetLastError(pClient);
        }

        /// <summary>
        /// 获取错误信息
        /// </summary>
        /// <returns></returns>
        public string GetLastErrorDesc()
        {
            IntPtr ptr = HPSocketSdk.HP_Server_GetLastErrorDesc(pClient);
            string desc = Marshal.PtrToStringUni(ptr);
            return desc;
        }

        // 是否启动
        public bool HasStarted()
        {
            if (pClient == IntPtr.Zero)
            {
                return false;
            }
            return HPSocketSdk.HP_Client_HasStarted(pClient);
        }

        /// <summary>
        /// 获取状态
        /// </summary>
        /// <returns></returns>
        public En_HP_ServiceState GetState()
        {
            return HPSocketSdk.HP_Client_GetState(pClient);
        }

        /// <summary>
        /// 获取监听socket的地址信息
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="ipLength"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public bool GetListenAddress(ref string ip, ref ushort port)
        {
            int ipLength = 40;

            StringBuilder sb = new StringBuilder(ipLength);

            bool ret = HPSocketSdk.HP_Client_GetLocalAddress(pClient, sb, ref ipLength, ref port);
            if (ret == true)
            {
                ip = sb.ToString();
            }
            return ret;
        }

        /// <summary>
        /// 获取该组件对象的连接Id
        /// </summary>
        /// <returns></returns>
        public uint GetConnectionId()
        {
            return HPSocketSdk.HP_Client_GetConnectionID(pClient);
        }

        ///////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 设置内存块缓存池大小（通常设置为 -> PUSH 模型：5 - 10；PULL 模型：10 - 20 ）
        /// </summary>
        /// <param name="val"></param>
        public void SetFreeBufferPoolSize(uint val)
        {
            HPSocketSdk.HP_Client_SetFreeBufferPoolSize(pClient, val);
        }

        /// <summary>
        ///  设置内存块缓存池回收阀值（通常设置为内存块缓存池大小的 3 倍）
        /// </summary>
        /// <param name="val"></param>
        public void SetFreeBufferPoolHold(uint val)
        {
            HPSocketSdk.HP_Client_SetFreeBufferPoolHold(pClient, val);
        }

        /// <summary>
        /// 获取内存块缓存池大小
        /// </summary>
        /// <returns></returns>
        public uint GetFreeBufferPoolSize()
        {
            return HPSocketSdk.HP_Client_GetFreeBufferPoolSize(pClient);
        }

        /// <summary>
        /// 获取内存块缓存池回收阀值
        /// </summary>
        /// <returns></returns>
        public uint GetFreeBufferPoolHold()
        {
            return HPSocketSdk.HP_Client_GetFreeBufferPoolHold(pClient);
        }

        ///////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 设置通信数据缓冲区大小（根据平均通信数据包大小调整设置，通常设置为：(N * 1024) - sizeof(TBufferObj)）
        /// </summary>
        /// <param name="val"></param>
        public void SetSocketBufferSize(uint val)
        {
            HPSocketSdk.HP_TcpClient_SetSocketBufferSize(pClient, val);
        }

        /// <summary>
        /// 设置心跳包间隔（毫秒，0 则不发送心跳包）
        /// </summary>
        /// <param name="val"></param>
        public void SetKeepAliveTime(uint val)
        {
            HPSocketSdk.HP_TcpClient_SetKeepAliveTime(pClient, val);
        }

        /// <summary>
        /// 设置心跳确认包检测间隔（毫秒，0 不发送心跳包，如果超过若干次 [默认：WinXP 5 次, Win7 10 次] 检测不到心跳确认包则认为已断线）
        /// </summary>
        /// <returns></returns>
        public void SetKeepAliveInterval(uint val)
        {
            HPSocketSdk.HP_TcpClient_SetKeepAliveInterval(pClient, val);
        }

        /// <summary>
        /// 获取通信数据缓冲区大小
        /// </summary>
        /// <returns></returns>
        public uint GetSocketBufferSize()
        {
            return HPSocketSdk.HP_TcpClient_GetSocketBufferSize(pClient);
        }

        /// <summary>
        /// 获取心跳检查次数
        /// </summary>
        /// <returns></returns>
        public uint GetKeepAliveTime()
        {
            return HPSocketSdk.HP_TcpClient_GetKeepAliveTime(pClient);
        }

        /// <summary>
        /// 获取心跳检查间隔
        /// </summary>
        /// <returns></returns>
        public uint GetKeepAliveInterval()
        {
            return HPSocketSdk.HP_TcpClient_GetKeepAliveInterval(pClient);
        }

        ///////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 设置回调函数
        /// </summary>
        /// <param name="prepareConnect"></param>
        /// <param name="connect"></param>
        /// <param name="send"></param>
        /// <param name="recv"></param>
        /// <param name="close"></param>
        /// <param name="error"></param>
        public void SetCallback(HPSocketSdk.HP_FN_OnPrepareConnect prepareConnect, HPSocketSdk.HP_FN_OnConnect connect, 
            HPSocketSdk.HP_FN_OnSend send, HPSocketSdk.HP_FN_OnReceive recv, HPSocketSdk.HP_FN_OnClose close, 
            HPSocketSdk.HP_FN_OnError error)
        {
            if (IsSetCallback == true)
            {
                throw new Exception("已经调用过SetCallback()方法,如果您确定没手动调用过该方法,并想要手动设置各回调函数,请在构造该类构造函数中传false值,并再次调用该方法。");
            }


            // 设置 Socket 监听器回调函数
            OnConnectCallback = new HPSocketSdk.HP_FN_OnConnect(connect);
            OnSendCallback = new HPSocketSdk.HP_FN_OnSend(send);
            OnPrepareConnectCallback = new HPSocketSdk.HP_FN_OnPrepareConnect(prepareConnect);
            OnReceiveCallback = new HPSocketSdk.HP_FN_OnReceive(recv);
            OnCloseCallback = new HPSocketSdk.HP_FN_OnClose(close);
            OnErrorCallback = new HPSocketSdk.HP_FN_OnError(error);

            // 设置 Socket 监听器回调函数
            HPSocketSdk.HP_Set_FN_Client_OnPrepareConnect(pListener, OnPrepareConnectCallback);
            HPSocketSdk.HP_Set_FN_Client_OnConnect(pListener, OnConnectCallback);
            HPSocketSdk.HP_Set_FN_Client_OnSend(pListener, OnSendCallback);
            HPSocketSdk.HP_Set_FN_Client_OnReceive(pListener, OnReceiveCallback);
            HPSocketSdk.HP_Set_FN_Client_OnClose(pListener, OnCloseCallback);
            HPSocketSdk.HP_Set_FN_Client_OnError(pListener, OnErrorCallback);

            IsSetCallback = true;
        }

        public virtual void SetOnErrorCallback(HPSocketSdk.HP_FN_OnError error)
        {
            OnErrorCallback = new HPSocketSdk.HP_FN_OnError(error);
            HPSocketSdk.HP_Set_FN_Server_OnError(pListener, OnErrorCallback);
        }

        public virtual void SetOnCloseCallback(HPSocketSdk.HP_FN_OnClose close)
        {
            OnCloseCallback = new HPSocketSdk.HP_FN_OnClose(close);
            HPSocketSdk.HP_Set_FN_Server_OnClose(pListener, OnCloseCallback);
        }

        public virtual void SetOnReceiveCallback(HPSocketSdk.HP_FN_OnReceive recv)
        {
            OnReceiveCallback = new HPSocketSdk.HP_FN_OnReceive(recv);
            HPSocketSdk.HP_Set_FN_Server_OnReceive(pListener, OnReceiveCallback);
        }

        public virtual void SetOnPrepareConnectCallback(HPSocketSdk.HP_FN_OnPrepareConnect prepareConnect)
        {
            OnPrepareConnectCallback = new HPSocketSdk.HP_FN_OnPrepareConnect(prepareConnect);
            HPSocketSdk.HP_Set_FN_Agent_OnPrepareConnect(pListener, OnPrepareConnectCallback);
        }

        public virtual void SetOnConnectCallback(HPSocketSdk.HP_FN_OnConnect connect)
        {
            OnConnectCallback = new HPSocketSdk.HP_FN_OnConnect(connect);
            HPSocketSdk.HP_Set_FN_Agent_OnConnect(pListener, OnConnectCallback);
        }

        public virtual void SetOnSendCallback(HPSocketSdk.HP_FN_OnSend send)
        {
            OnSendCallback = new HPSocketSdk.HP_FN_OnSend(send);
            HPSocketSdk.HP_Set_FN_Server_OnSend(pListener, OnSendCallback);
        }
        //////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 准备连接了 到达一次
        /// </summary>
        /// <param name="dwConnID"></param>
        /// <param name="socket"></param>
        /// <returns></returns>
        protected virtual En_HP_HandleResult OnPrepareConnect(uint dwConnID, uint socket)
        {
            return En_HP_HandleResult.HP_HR_OK;
        }

        /// <summary>
        /// 已连接 到达一次
        /// </summary>
        /// <param name="dwConnID"></param>
        /// <returns></returns>
        protected virtual En_HP_HandleResult OnConnect(uint dwConnID)
        {
            return En_HP_HandleResult.HP_HR_OK;
        }

        /// <summary>
        /// 客户端发数据了
        /// </summary>
        /// <param name="dwConnID"></param>
        /// <param name="pData"></param>
        /// <param name="iLength"></param>
        /// <returns></returns>
        protected virtual En_HP_HandleResult OnSend(uint dwConnID, IntPtr pData, int iLength)
        {
            return En_HP_HandleResult.HP_HR_OK;
        }

        /// <summary>
        /// 数据到达了
        /// </summary>
        /// <param name="dwConnID"></param>
        /// <param name="pData"></param>
        /// <param name="iLength"></param>
        /// <returns></returns>
        protected virtual En_HP_HandleResult OnReceive(uint dwConnID, IntPtr pData, int iLength)
        {
            return En_HP_HandleResult.HP_HR_OK;
        }

        /// <summary>
        /// 连接关闭了
        /// </summary>
        /// <param name="dwConnID"></param>
        /// <returns></returns>
        protected virtual En_HP_HandleResult OnClose(uint dwConnID)
        {
            return En_HP_HandleResult.HP_HR_OK;
        }

        /// <summary>
        /// 出错了
        /// </summary>
        /// <param name="dwConnID"></param>
        /// <param name="enOperation"></param>
        /// <param name="iErrorCode"></param>
        /// <returns></returns>
        protected virtual En_HP_HandleResult OnError(uint dwConnID, En_HP_SocketOperation enOperation, int iErrorCode)
        {
            return En_HP_HandleResult.HP_HR_OK;
        }
    }
}
