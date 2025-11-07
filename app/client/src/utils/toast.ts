import toast from 'react-hot-toast';

export const showToast = {
  success: (message: string) => 
    toast.success(message, {
      duration: 4000,
      style: {
        background: '#065f46',
        color: '#fff',
        fontWeight: '500',
      },
      iconTheme: {
        primary: '#10b981',
        secondary: '#fff',
      },
    }),
  
  error: (message: string) => 
    toast.error(message, {
      duration: Infinity,
      style: {
        background: '#991b1b',
        color: '#fff',
        fontWeight: '500',
      },
      iconTheme: {
        primary: '#ef4444',
        secondary: '#fff',
      },
    }),
  
  loading: (message: string) => 
    toast.loading(message, {
      style: {
        background: '#064e3b',
        color: '#fff',
        fontWeight: '500',
      },
      iconTheme: {
        primary: '#10b981',
        secondary: '#fff',
      },
    }),
  
  promise: <T,>(
    promise: Promise<T>,
    messages: {
      loading: string;
      success: string;
      error: string;
    }
  ) => toast.promise(promise, messages, {
    success: {
      duration: 4000,
      style: {
        background: '#065f46',
        color: '#fff',
        fontWeight: '500',
      },
      iconTheme: {
        primary: '#10b981',
        secondary: '#fff',
      },
    },
    error: {
      duration: Infinity,
      style: {
        background: '#991b1b',
        color: '#fff',
        fontWeight: '500',
      },
      iconTheme: {
        primary: '#ef4444',
        secondary: '#fff',
      },
    },
    loading: {
      style: {
        background: '#064e3b',
        color: '#fff',
        fontWeight: '500',
      },
      iconTheme: {
        primary: '#10b981',
        secondary: '#fff',
      },
    },
  }),
};
