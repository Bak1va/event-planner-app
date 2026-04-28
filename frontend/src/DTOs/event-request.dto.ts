export interface EventRequest {
  name: string;
  status: string;
  eventDate: string;
  description?: string;
  imageUrl?: string;
  userId?: number;
}
