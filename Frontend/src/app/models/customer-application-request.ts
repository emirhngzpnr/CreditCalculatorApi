export interface CustomerApplicationRequest {
  name: string;
  surName: string;
  identityNumber: string;
  phone: string;
  email: string;
  birthDate: string; // string formatta gönderiyoruz: "2025-07-28T00:00:00"
  bankName: string;
  customerNumber:string;
}
