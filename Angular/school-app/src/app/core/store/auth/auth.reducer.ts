import { createReducer, on } from '@ngrx/store';
import { AuthState, initialAuthState } from './auth.state';
import * as AuthActions from './auth.actions';

export const authReducer = createReducer(
  initialAuthState,

  // Login
  on(AuthActions.login, state => ({
    ...state,
    isLoading: true,
    error: null
  })),

  on(AuthActions.loginSuccess, (state, { response }) => ({
    ...state,
    isLoading: false,
    error: null,
    isAuthenticated: true,
    accessToken: response.accessToken,
    refreshToken: response.refreshToken,
    user: {
      userId: '',
      email: response.email,
      fullName: response.fullName,
      role: response.role
    }
  })),

  on(AuthActions.loginFailure, (state, { error }) => ({
    ...state,
    isLoading: false,
    error,
    isAuthenticated: false,
    user: null,
    accessToken: null,
    refreshToken: null
  })),

  // Logout
  on(AuthActions.logoutSuccess, () => ({
    ...initialAuthState
  })),

  // Refresh token success
  on(AuthActions.refreshTokenSuccess, (state, { response }) => ({
    ...state,
    accessToken: response.accessToken,
    refreshToken: response.refreshToken,
    isAuthenticated: true
  })),

  on(AuthActions.refreshTokenFailure, () => ({
    ...initialAuthState
  }))
);