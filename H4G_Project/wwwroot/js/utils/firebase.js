import { initializeApp } from 'https://www.gstatic.com/firebasejs/10.5.2/firebase-app.js'

// Add Firebase products that you want to use
import { getAuth, createUserWithEmailAndPassword, signInWithEmailAndPassword } from 'https://www.gstatic.com/firebasejs/10.5.2/firebase-auth.js'
import { getFirestore } from 'https://www.gstatic.com/firebasejs/10.5.2/firebase-firestore.js'

// For Firebase JS SDK v7.20.0 and later, measurementId is optional
const firebaseConfig = {
  apiKey: "AIzaSyBDT4DVv_xmS1C_1WTOpmnlU4WqmqhrOfI",
  authDomain: "squad-60b0b.firebaseapp.com",
  projectId: "squad-60b0b",
  storageBucket: "squad-60b0b.firebasestorage.app",
  messagingSenderId: "219668334828",
  appId: "1:219668334828:web:5df0f3df3ed9a17e5a7dc9",
  measurementId: "G-14S6QHE3KK"
};

const app = initializeApp(firebaseConfig);


const db = getFirestore(app);
const auth = getAuth(app);

export { db, auth };