import { UserInfo } from "../user/userInfo";

export interface AuthResponse {
  accessToken: string;
  idToken: string;
  refreshToken: string;
  expiresIn: number;
  user: UserInfo
}
