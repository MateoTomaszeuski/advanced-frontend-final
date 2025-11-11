import { describe, it, expect, beforeEach } from 'vitest';
import { useAgentStore } from '../../stores/useAgentStore';

describe('useAgentStore', () => {
  beforeEach(() => {
    useAgentStore.setState({
      status: 'idle',
      currentTask: null,
      progress: 0,
      currentConversation: null,
      conversations: [],
      recentActions: [],
    });
  });

  it('should initialize with default state', () => {
    const state = useAgentStore.getState();
    
    expect(state.status).toBe('idle');
    expect(state.currentTask).toBeNull();
    expect(state.progress).toBe(0);
    expect(state.currentConversation).toBeNull();
    expect(state.conversations).toEqual([]);
    expect(state.recentActions).toEqual([]);
  });

  it('should update status', () => {
    const { setStatus } = useAgentStore.getState();
    
    setStatus('processing');
    
    expect(useAgentStore.getState().status).toBe('processing');
  });

  it('should update current task', () => {
    const { setCurrentTask } = useAgentStore.getState();
    
    setCurrentTask('Creating playlist');
    
    expect(useAgentStore.getState().currentTask).toBe('Creating playlist');
  });

  it('should update progress', () => {
    const { setProgress } = useAgentStore.getState();
    
    setProgress(50);
    
    expect(useAgentStore.getState().progress).toBe(50);
  });

  it('should set current conversation', () => {
    const { setCurrentConversation } = useAgentStore.getState();
    const conversation = {
      id: 1,
      userId: 1,
      title: 'Test Conversation',
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
    };
    
    setCurrentConversation(conversation);
    
    expect(useAgentStore.getState().currentConversation).toEqual(conversation);
  });

  it('should add conversation', () => {
    const { addConversation } = useAgentStore.getState();
    const conversation = {
      id: 1,
      userId: 1,
      title: 'New Conversation',
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
    };
    
    addConversation(conversation);
    
    const state = useAgentStore.getState();
    expect(state.conversations).toHaveLength(1);
    expect(state.conversations[0]).toEqual(conversation);
  });

  it('should add recent action', () => {
    const { addRecentAction } = useAgentStore.getState();
    const action = {
      id: 1,
      conversationId: 1,
      actionType: 'CreateSmartPlaylist',
      status: 'Completed' as const,
      createdAt: new Date().toISOString(),
    };
    
    addRecentAction(action);
    
    const state = useAgentStore.getState();
    expect(state.recentActions).toHaveLength(1);
    expect(state.recentActions[0]).toEqual(action);
  });

  it('should limit recent actions to 10', () => {
    const { addRecentAction } = useAgentStore.getState();
    
    for (let i = 0; i < 15; i++) {
      addRecentAction({
        id: i,
        conversationId: 1,
        actionType: 'Test',
        status: 'Completed' as const,
        createdAt: new Date().toISOString(),
      });
    }
    
    const state = useAgentStore.getState();
    expect(state.recentActions).toHaveLength(10);
  });

  it('should clear recent actions', () => {
    const { addRecentAction, clearRecentActions } = useAgentStore.getState();
    
    addRecentAction({
      id: 1,
      conversationId: 1,
      actionType: 'Test',
      status: 'Completed' as const,
      createdAt: new Date().toISOString(),
    });
    
    clearRecentActions();
    
    expect(useAgentStore.getState().recentActions).toEqual([]);
  });

  it('should set conversations', () => {
    const { setConversations } = useAgentStore.getState();
    const conversations = [
      {
        id: 1,
        userId: 1,
        title: 'Conv 1',
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString(),
      },
      {
        id: 2,
        userId: 1,
        title: 'Conv 2',
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString(),
      },
    ];
    
    setConversations(conversations);
    
    expect(useAgentStore.getState().conversations).toEqual(conversations);
  });
});
