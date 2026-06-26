import { createAction, props } from '@ngrx/store';
import { LoginRequest, LoginResponse, AuthUser } from '../../models/auth.models';

// Login
export const login = createAction(
  '[Auth] Login',
  props<{ request: LoginRequest }>()
);

export const loginSuccess = createAction(
  '[Auth] Login Success',
  props<{ response: LoginResponse }>()
);

export const loginFailure = createAction(
  '[Auth] Login Failure',
  props<{ error: string }>()
);

// Logout
export const logout = createAction('[Auth] Logout');

export const logoutSuccess = createAction('[Auth] Logout Success');

// Refresh Token
export const refreshToken = createAction('[Auth] Refresh Token');

export const refreshTokenSuccess = createAction(
  '[Auth] Refresh Token Success',
  props<{ response: LoginResponse }>()
);

export const refreshTokenFailure = createAction(
  '[Auth] Refresh Token Failure'
);

// Load from storage on app start
export const loadAuthFromStorage = createAction('[Auth] Load From Storage');